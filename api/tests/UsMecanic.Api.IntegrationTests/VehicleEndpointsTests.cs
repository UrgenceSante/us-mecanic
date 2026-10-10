using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private static readonly Uri Vehicles = new("/api/vehicles", UriKind.Relative);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public VehicleEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record VehicleDto(Guid Id, string LicensePlate, string? Brand, string? Model, DateOnly? FirstRegistrationDate, string? Vin, string? InternalNumber, string Status);

    private sealed record ImportLine(int Line, string? LicensePlate, string Outcome, string? ErrorCode);

    private sealed record ImportReport(bool DryRun, int Created, int Updated, int Unchanged, int Errors, List<ImportLine> Lines);

    /// <summary>Plaque SIV unique : les tests d'une classe partagent la même base.</summary>
    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private async Task<VehicleDto> CreateAsync(string plate, string? brand = "Renault")
    {
        var response = await _client.PostAsJsonAsync(Vehicles, new { licensePlate = plate, details = new { brand, model = "Master", firstRegistrationDate = "2021-03-15" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!;
    }

    [Fact]
    public async Task Les_vehicules_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(Vehicles);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creation_puis_lecture_et_recherche()
    {
        var plate = NewPlate();
        var created = await CreateAsync(plate.Replace("-", " ", StringComparison.Ordinal).ToLowerInvariant(), "Volkswagen");

        Assert.Equal(plate, created.LicensePlate);
        Assert.Equal("Active", created.Status);
        Assert.Equal(new DateOnly(2021, 3, 15), created.FirstRegistrationDate);

        var read = await _client.GetFromJsonAsync<VehicleDto>(new Uri($"/api/vehicles/{created.Id}", UriKind.Relative), Json);
        Assert.Equal(created, read);

        var found = await _client.GetFromJsonAsync<List<VehicleDto>>(new Uri($"/api/vehicles?search={plate.Replace("-", "", StringComparison.Ordinal)}", UriKind.Relative), Json);
        Assert.Equal(created.Id, Assert.Single(found!).Id);
    }

    [Fact]
    public async Task Une_immatriculation_en_double_renvoie_un_conflit_ProblemDetails()
    {
        var plate = NewPlate();
        await CreateAsync(plate);

        var response = await _client.PostAsJsonAsync(Vehicles, new { licensePlate = plate, details = new { } });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("vehicle.plate_already_exists", problem.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Une_saisie_invalide_renvoie_400_et_un_vehicule_inconnu_404()
    {
        var invalid = await _client.PostAsJsonAsync(Vehicles, new { licensePlate = "invalide", details = new { } });
        var unknown = await _client.GetAsync(new Uri($"/api/vehicles/{Guid.NewGuid()}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Modification_et_changement_de_statut()
    {
        var vehicle = await CreateAsync(NewPlate());
        var uri = new Uri($"/api/vehicles/{vehicle.Id}", UriKind.Relative);

        var updated = await _client.PutAsJsonAsync(uri, new { brand = "Renault", model = "Master L3H2", vin = "VF1MA000012345678" });
        var immobilized = await _client.PutAsJsonAsync(new Uri($"{uri}/status", UriKind.Relative), new { status = "Immobilized" });

        Assert.Equal("Master L3H2", (await updated.Content.ReadFromJsonAsync<VehicleDto>(Json))!.Model);
        Assert.Equal("Immobilized", (await immobilized.Content.ReadFromJsonAsync<VehicleDto>(Json))!.Status);
        var list = await _client.GetFromJsonAsync<List<VehicleDto>>(new Uri("/api/vehicles?status=Immobilized", UriKind.Relative), Json);
        Assert.Contains(list!, v => v.Id == vehicle.Id);
    }

    [Fact]
    public async Task Import_CSV_en_simulation_puis_reel()
    {
        var existing = await CreateAsync(NewPlate(), brand: null);
        var newPlate = NewPlate();
        var csv = $"Immatriculation;Marque;Modèle\n{existing.LicensePlate};Renault;\n{newPlate};Peugeot;Boxer\nfausse;;\n";

        var simulation = await ImportAsync(csv, dryRun: true);
        Assert.Equal((true, 1, 1, 1), (simulation.DryRun, simulation.Created, simulation.Updated, simulation.Errors));
        var afterSimulation = await _client.GetFromJsonAsync<List<VehicleDto>>(new Uri($"/api/vehicles?search={newPlate}", UriKind.Relative), Json);
        Assert.Empty(afterSimulation!);

        var import = await ImportAsync(csv, dryRun: false);
        Assert.Equal((false, 1, 1), (import.DryRun, import.Created, import.Updated));
        Assert.Equal("license_plate.invalid_format", import.Lines.Single(l => l.Line == 4).ErrorCode);

        var refreshed = await _client.GetFromJsonAsync<VehicleDto>(new Uri($"/api/vehicles/{existing.Id}", UriKind.Relative), Json);
        Assert.Equal(("Renault", "Master"), (refreshed!.Brand, refreshed.Model));
    }

    [Fact]
    public async Task Import_sans_colonne_immatriculation_renvoie_400()
    {
        var response = await PostImportAsync("marque\nRenault", dryRun: true);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<ImportReport> ImportAsync(string csv, bool dryRun)
    {
        var response = await PostImportAsync(csv, dryRun);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ImportReport>(Json))!;
    }

    private async Task<HttpResponseMessage> PostImportAsync(string csv, bool dryRun)
    {
        using var form = new MultipartFormDataContent();
        using var file = new ByteArrayContent(Encoding.UTF8.GetBytes(csv));
        form.Add(file, "file", "parc.csv");
        return await _client.PostAsync(new Uri($"/api/vehicles/import?dryRun={dryRun.ToString().ToLowerInvariant()}", UriKind.Relative), form);
    }
}
