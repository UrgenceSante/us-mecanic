using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class MileageReadingEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public MileageReadingEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record ReadingDto(Guid Id, Guid VehicleId, DateOnly Date, int Kilometers, string? Reason, DateTimeOffset RecordedAt);

    private sealed record VehicleDto(Guid Id);

    /// <summary>Date passée (jamais aujourd'hui, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly DaysAgo(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

    /// <summary>Plaque SIV unique : les tests d'une classe partagent la même base.</summary>
    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri ReadingsUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/mileage-readings", UriKind.Relative);

    private static Uri ReadingUri(Guid vehicleId, Guid readingId) => new($"/api/vehicles/{vehicleId}/mileage-readings/{readingId}", UriKind.Relative);

    private async Task<Guid> CreateVehicleAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = NewPlate(), details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!.Id;
    }

    private async Task<ReadingDto> AddAsync(Guid vehicleId, DateOnly date, int kilometers)
    {
        var response = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date, kilometers });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ReadingDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_releves_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(ReadingsUri(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Ajout_puis_liste_du_plus_recent_au_plus_ancien()
    {
        var vehicleId = await CreateVehicleAsync();

        var response = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(10), kilometers = 10_000, reason = "  Mise en service " });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<ReadingDto>(Json))!;
        Assert.Equal(ReadingUri(vehicleId, created.Id).ToString(), response.Headers.Location?.ToString());
        Assert.Equal(vehicleId, created.VehicleId);
        Assert.Equal(DaysAgo(10), created.Date);
        Assert.Equal(10_000, created.Kilometers);
        Assert.Equal("Mise en service", created.Reason);

        await AddAsync(vehicleId, DaysAgo(2), 10_500);
        await AddAsync(vehicleId, DaysAgo(5), 10_200);

        var list = await _client.GetFromJsonAsync<List<ReadingDto>>(ReadingsUri(vehicleId), Json);
        Assert.Equal([10_500, 10_200, 10_000], list!.Select(r => r.Kilometers));
    }

    [Fact]
    public async Task Un_vehicule_inconnu_renvoie_404()
    {
        var unknown = Guid.NewGuid();

        var list = await _client.GetAsync(ReadingsUri(unknown));
        var add = await _client.PostAsJsonAsync(ReadingsUri(unknown), new { date = DaysAgo(2), kilometers = 1 });

        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(list));
        Assert.Equal(HttpStatusCode.NotFound, add.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(add));
    }

    [Fact]
    public async Task Une_date_future_est_refusee()
    {
        var vehicleId = await CreateVehicleAsync();

        var response = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(-2), kilometers = 100 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("mileage_reading.date_in_future", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Un_kilometrage_negatif_est_refuse()
    {
        var vehicleId = await CreateVehicleAsync();

        var response = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(2), kilometers = -5 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("mileage_reading.kilometers_negative", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Un_kilometrage_incoherent_exige_un_motif()
    {
        var vehicleId = await CreateVehicleAsync();
        await AddAsync(vehicleId, DaysAgo(10), 10_000);
        await AddAsync(vehicleId, DaysAgo(2), 10_400);

        var lower = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(5), kilometers = 9_000 });
        var higher = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(5), kilometers = 10_900 });
        var withReason = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(5), kilometers = 9_000, reason = "Compteur remplacé" });

        Assert.Equal(HttpStatusCode.BadRequest, lower.StatusCode);
        Assert.Equal("mileage_reading.reason_required", await ErrorCodeAsync(lower));
        Assert.Equal(HttpStatusCode.BadRequest, higher.StatusCode);
        Assert.Equal("mileage_reading.reason_required", await ErrorCodeAsync(higher));
        Assert.Equal(HttpStatusCode.Created, withReason.StatusCode);
    }

    [Fact]
    public async Task Plus_de_1000_km_par_jour_exige_une_confirmation_explicite()
    {
        var vehicleId = await CreateVehicleAsync();
        await AddAsync(vehicleId, DaysAgo(4), 10_000);

        var warning = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(2), kilometers = 12_500 });
        var confirmed = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(2), kilometers = 12_500, confirmHighMileage = true });

        Assert.Equal(HttpStatusCode.BadRequest, warning.StatusCode);
        Assert.Equal("mileage_reading.high_mileage_confirmation_required", await ErrorCodeAsync(warning));
        Assert.Equal(HttpStatusCode.Created, confirmed.StatusCode);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_refuse_les_ecritures_mais_reste_consultable()
    {
        var vehicleId = await CreateVehicleAsync();
        var reading = await AddAsync(vehicleId, DaysAgo(5), 1_000);
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        var add = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = DaysAgo(2), kilometers = 1_100 });
        var update = await _client.PutAsJsonAsync(ReadingUri(vehicleId, reading.Id), new { date = DaysAgo(5), kilometers = 1_050 });
        var delete = await _client.DeleteAsync(ReadingUri(vehicleId, reading.Id));
        var list = await _client.GetAsync(ReadingsUri(vehicleId));

        Assert.Equal(HttpStatusCode.Conflict, add.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(add));
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Single((await list.Content.ReadFromJsonAsync<List<ReadingDto>>(Json))!);
    }

    [Fact]
    public async Task La_modification_applique_les_memes_controles_et_ignore_le_releve_modifie()
    {
        var vehicleId = await CreateVehicleAsync();
        await AddAsync(vehicleId, DaysAgo(10), 5_000);
        var middle = await AddAsync(vehicleId, DaysAgo(5), 5_300);
        await AddAsync(vehicleId, DaysAgo(1), 9_000);

        var ok = await _client.PutAsJsonAsync(ReadingUri(vehicleId, middle.Id), new { date = DaysAgo(5), kilometers = 5_100 });
        var lower = await _client.PutAsJsonAsync(ReadingUri(vehicleId, middle.Id), new { date = DaysAgo(5), kilometers = 4_900 });
        var future = await _client.PutAsJsonAsync(ReadingUri(vehicleId, middle.Id), new { date = DaysAgo(-3), kilometers = 5_300 });
        var fast = await _client.PutAsJsonAsync(ReadingUri(vehicleId, middle.Id), new { date = DaysAgo(9), kilometers = 6_500 });
        var withReason = await _client.PutAsJsonAsync(ReadingUri(vehicleId, middle.Id), new { date = DaysAgo(5), kilometers = 4_900, reason = "Correction" });

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        var updated = (await ok.Content.ReadFromJsonAsync<ReadingDto>(Json))!;
        Assert.Equal(middle.Id, updated.Id);
        Assert.Equal(5_100, updated.Kilometers);
        Assert.Equal(HttpStatusCode.BadRequest, lower.StatusCode);
        Assert.Equal("mileage_reading.reason_required", await ErrorCodeAsync(lower));
        Assert.Equal(HttpStatusCode.BadRequest, future.StatusCode);
        Assert.Equal("mileage_reading.date_in_future", await ErrorCodeAsync(future));
        Assert.Equal(HttpStatusCode.BadRequest, fast.StatusCode);
        Assert.Equal("mileage_reading.high_mileage_confirmation_required", await ErrorCodeAsync(fast));
        Assert.Equal(HttpStatusCode.OK, withReason.StatusCode);
    }

    [Fact]
    public async Task Supprimer_un_releve_le_retire_de_l_historique()
    {
        var vehicleId = await CreateVehicleAsync();
        var reading = await AddAsync(vehicleId, DaysAgo(5), 1_000);

        var deleted = await _client.DeleteAsync(ReadingUri(vehicleId, reading.Id));
        var again = await _client.DeleteAsync(ReadingUri(vehicleId, reading.Id));
        var list = await _client.GetFromJsonAsync<List<ReadingDto>>(ReadingsUri(vehicleId), Json);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
        Assert.Equal("mileage_reading.not_found", await ErrorCodeAsync(again));
        Assert.Empty(list!);
    }

    [Fact]
    public async Task Un_releve_est_introuvable_via_un_autre_vehicule()
    {
        var owner = await CreateVehicleAsync();
        var other = await CreateVehicleAsync();
        var reading = await AddAsync(owner, DaysAgo(5), 1_000);

        var update = await _client.PutAsJsonAsync(ReadingUri(other, reading.Id), new { date = DaysAgo(5), kilometers = 1_100 });

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal("mileage_reading.not_found", await ErrorCodeAsync(update));
    }
}
