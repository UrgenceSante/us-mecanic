using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleTypeEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private static readonly Uri Types = new("/api/vehicle-types", UriKind.Relative);
    private static readonly Uri Vehicles = new("/api/vehicles", UriKind.Relative);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public VehicleTypeEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record TypeDto(Guid Id, string Name, bool IsActive);

    private sealed record VehicleDto(Guid Id, string LicensePlate, Guid? TypeId, string? TypeName);

    /// <summary>Nom unique : les tests d'une classe partagent la même base.</summary>
    private static string NewName(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    /// <summary>Plaque SIV unique, même principe que dans les tests de véhicules.</summary>
    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private async Task<TypeDto> CreateTypeAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(Types, new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TypeDto>(Json))!;
    }

    private async Task<HttpResponseMessage> CreateVehicleAsync(string plate, Guid? typeId) =>
        await _client.PostAsJsonAsync(Vehicles, new { licensePlate = plate, details = new { brand = "Renault", typeId } });

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_types_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(Types);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creation_puis_liste_et_renommage()
    {
        var created = await CreateTypeAsync($"  {NewName("Ambulance")} ");
        Assert.True(created.IsActive);
        Assert.StartsWith("Ambulance ", created.Name, StringComparison.Ordinal);

        var list = await _client.GetFromJsonAsync<List<TypeDto>>(Types, Json);
        Assert.Contains(list!, t => t.Id == created.Id);

        var newName = NewName("VSL");
        var renamed = await _client.PutAsJsonAsync(new Uri($"/api/vehicle-types/{created.Id}", UriKind.Relative), new { name = newName });
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal(newName, (await renamed.Content.ReadFromJsonAsync<TypeDto>(Json))!.Name);
    }

    [Fact]
    public async Task Un_nom_vide_ou_en_double_renvoie_un_ProblemDetails()
    {
        var name = NewName("Sang");
        await CreateTypeAsync(name);

        var empty = await _client.PostAsJsonAsync(Types, new { name = " " });
        var duplicate = await _client.PostAsJsonAsync(Types, new { name = name.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal("vehicle_type.name_required", await ErrorCodeAsync(empty));
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("vehicle_type.name_already_exists", await ErrorCodeAsync(duplicate));
    }

    [Fact]
    public async Task Renommer_vers_un_nom_existant_ou_un_type_inconnu_est_refuse()
    {
        var other = await CreateTypeAsync(NewName("Autre"));
        var type = await CreateTypeAsync(NewName("Type"));

        var duplicate = await _client.PutAsJsonAsync(new Uri($"/api/vehicle-types/{type.Id}", UriKind.Relative), new { name = other.Name });
        var unknown = await _client.PutAsJsonAsync(new Uri($"/api/vehicle-types/{Guid.NewGuid()}", UriKind.Relative), new { name = NewName("X") });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Le_type_est_accepte_sur_le_vehicule_et_filtre_la_liste()
    {
        var type = await CreateTypeAsync(NewName("Ambulance"));
        var typedPlate = NewPlate();
        var plainPlate = NewPlate();

        var typed = await CreateVehicleAsync(typedPlate, type.Id);
        var plain = await CreateVehicleAsync(plainPlate, null);

        Assert.Equal(HttpStatusCode.Created, typed.StatusCode);
        Assert.Equal(HttpStatusCode.Created, plain.StatusCode);
        var created = (await typed.Content.ReadFromJsonAsync<VehicleDto>(Json))!;
        Assert.Equal((type.Id, type.Name), (created.TypeId, created.TypeName));

        var filtered = await _client.GetFromJsonAsync<List<VehicleDto>>(new Uri($"/api/vehicles?type={type.Id}", UriKind.Relative), Json);
        Assert.Equal(created.Id, Assert.Single(filtered!).Id);

        var changed = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{created.Id}", UriKind.Relative), new { brand = "Renault", typeId = (Guid?)null });
        Assert.Null((await changed.Content.ReadFromJsonAsync<VehicleDto>(Json))!.TypeId);
    }

    [Fact]
    public async Task Un_type_inconnu_est_refuse_sur_un_vehicule()
    {
        var response = await CreateVehicleAsync(NewPlate(), Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("vehicle_type.unknown", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Un_type_desactive_n_est_plus_propose_mais_reste_affiche_sur_ses_vehicules()
    {
        var type = await CreateTypeAsync(NewName("Ancien"));
        var vehicle = (await (await CreateVehicleAsync(NewPlate(), type.Id)).Content.ReadFromJsonAsync<VehicleDto>(Json))!;

        var deactivated = await _client.PostAsync(new Uri($"/api/vehicle-types/{type.Id}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
        Assert.False((await deactivated.Content.ReadFromJsonAsync<TypeDto>(Json))!.IsActive);

        var proposed = await _client.GetFromJsonAsync<List<TypeDto>>(Types, Json);
        var all = await _client.GetFromJsonAsync<List<TypeDto>>(new Uri("/api/vehicle-types?includeInactive=true", UriKind.Relative), Json);
        Assert.DoesNotContain(proposed!, t => t.Id == type.Id);
        Assert.Contains(all!, t => t.Id == type.Id && !t.IsActive);

        var read = await _client.GetFromJsonAsync<VehicleDto>(new Uri($"/api/vehicles/{vehicle.Id}", UriKind.Relative), Json);
        Assert.Equal(type.Name, read!.TypeName);

        var refused = await CreateVehicleAsync(NewPlate(), type.Id);
        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("vehicle_type.inactive", await ErrorCodeAsync(refused));
    }

    [Fact]
    public async Task Un_type_ne_peut_pas_etre_supprime()
    {
        var type = await CreateTypeAsync(NewName("Permanent"));

        var response = await _client.DeleteAsync(new Uri($"/api/vehicle-types/{type.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
