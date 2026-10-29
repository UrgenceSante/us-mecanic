using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class ProblemEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Uri ProblemsUri = new("/api/problems", UriKind.Relative);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public ProblemEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record VehicleDto(Guid Id);

    private sealed record ProblemDto(Guid Id, Guid VehicleId, DateOnly Date, string Description, string Author, string Status);

    /// <summary>Jour décalé par rapport à aujourd'hui (jamais 0, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly Offset(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri VehicleProblemsUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/problems", UriKind.Relative);

    private async Task<(Guid Id, string Plate)> CreateVehicleAsync()
    {
        var plate = NewPlate();
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = plate, details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return ((await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!.Id, plate);
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_problemes_exigent_une_authentification()
    {
        var write = await _anonymous.PostAsJsonAsync(ProblemsUri, new { licensePlate = "AB-123-CD", date = Offset(-1), description = "Panne", author = "D. Martin" });
        var read = await _anonymous.GetAsync(VehicleProblemsUri(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, read.StatusCode);
    }

    [Fact]
    public async Task Un_signalement_valide_cree_un_probleme_signale_liste_pour_le_vehicule()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();
        var date = Offset(-2);

        var response = await _client.PostAsJsonAsync(
            ProblemsUri, new { licensePlate = plate.Replace("-", " ", StringComparison.Ordinal).ToLowerInvariant(), date, description = "  Voyant moteur  ", author = "D. Martin" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/vehicles/{vehicleId}/problems", response.Headers.Location?.ToString());
        var created = (await response.Content.ReadFromJsonAsync<ProblemDto>(Json))!;
        Assert.Equal(vehicleId, created.VehicleId);
        Assert.Equal("Reported", created.Status);
        Assert.Equal("Voyant moteur", created.Description);
        Assert.Equal("D. Martin", created.Author);
        Assert.Equal(date, created.Date);

        var list = await _client.GetFromJsonAsync<List<ProblemDto>>(VehicleProblemsUri(vehicleId), Json);
        Assert.Equal(created.Id, Assert.Single(list!).Id);
    }

    [Fact]
    public async Task La_liste_ne_contient_que_les_problemes_du_vehicule_du_plus_recent_au_plus_ancien()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();
        var (otherId, otherPlate) = await CreateVehicleAsync();
        await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(-6), description = "Ancien", author = "A" });
        await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(-1), description = "Récent", author = "A" });
        await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = otherPlate, date = Offset(-3), description = "Autre", author = "A" });

        var list = await _client.GetFromJsonAsync<List<ProblemDto>>(VehicleProblemsUri(vehicleId), Json);
        var otherList = await _client.GetFromJsonAsync<List<ProblemDto>>(VehicleProblemsUri(otherId), Json);

        Assert.Equal(["Récent", "Ancien"], list!.Select(p => p.Description));
        Assert.Equal("Autre", Assert.Single(otherList!).Description);
    }

    [Fact]
    public async Task Les_signalements_invalides_sont_refuses_avec_leur_code()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();

        var noAuthor = await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(-1), description = "Panne", author = " " });
        var noAuthorField = await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(-1), description = "Panne" });
        var noDescription = await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(-1), description = "", author = "A" });
        var future = await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(5), description = "Panne", author = "A" });
        var badPlate = await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = "nimporte", date = Offset(-1), description = "Panne", author = "A" });

        Assert.Equal(HttpStatusCode.BadRequest, noAuthor.StatusCode);
        Assert.Equal("problem.author_required", await ErrorCodeAsync(noAuthor));
        Assert.Equal(HttpStatusCode.BadRequest, noAuthorField.StatusCode);
        Assert.Equal("problem.author_required", await ErrorCodeAsync(noAuthorField));
        Assert.Equal(HttpStatusCode.BadRequest, noDescription.StatusCode);
        Assert.Equal("problem.description_required", await ErrorCodeAsync(noDescription));
        Assert.Equal(HttpStatusCode.BadRequest, future.StatusCode);
        Assert.Equal("problem.date_in_future", await ErrorCodeAsync(future));
        Assert.Equal(HttpStatusCode.BadRequest, badPlate.StatusCode);
        Assert.Equal("license_plate.invalid_format", await ErrorCodeAsync(badPlate));
        Assert.Empty((await _client.GetFromJsonAsync<List<ProblemDto>>(VehicleProblemsUri(vehicleId), Json))!);
    }

    [Fact]
    public async Task Un_vehicule_inconnu_ou_sorti_du_parc_est_refuse_et_sa_liste_reste_lisible()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();
        await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(-2), description = "Avant sortie", author = "A" });

        var unknown = await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = NewPlate(), date = Offset(-1), description = "Panne", author = "A" });
        var unknownRead = await _client.GetAsync(VehicleProblemsUri(Guid.NewGuid()));
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var afterRetired = await _client.PostAsJsonAsync(ProblemsUri, new { licensePlate = plate, date = Offset(-1), description = "Panne", author = "A" });
        var readRetired = await _client.GetAsync(VehicleProblemsUri(vehicleId));

        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("problem.vehicle_unknown", await ErrorCodeAsync(unknown));
        Assert.Equal(HttpStatusCode.NotFound, unknownRead.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, afterRetired.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(afterRetired));
        Assert.Equal(HttpStatusCode.OK, readRetired.StatusCode);
        Assert.Single((await readRetired.Content.ReadFromJsonAsync<List<ProblemDto>>(Json))!);
    }
}
