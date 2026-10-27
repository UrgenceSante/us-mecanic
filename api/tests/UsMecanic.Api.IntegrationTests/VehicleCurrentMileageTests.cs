using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleCurrentMileageTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public VehicleCurrentMileageTests(ApiFactory factory) => _client = factory.CreateAuthenticatedClient();

    private sealed record VehicleDto(Guid Id, string LicensePlate, int? CurrentMileage, DateOnly? CurrentMileageDate);

    private sealed record ReadingDto(Guid Id);

    /// <summary>Date passée (jamais aujourd'hui, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly DaysAgo(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

    /// <summary>Plaque SIV unique : les tests d'une classe partagent la même base.</summary>
    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri VehicleUri(Guid id) => new($"/api/vehicles/{id}", UriKind.Relative);

    private static Uri ReadingsUri(Guid id) => new($"/api/vehicles/{id}/mileage-readings", UriKind.Relative);

    private async Task<VehicleDto> CreateVehicleAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = NewPlate(), details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!;
    }

    private async Task<ReadingDto> AddAsync(Guid vehicleId, DateOnly date, int kilometers)
    {
        var response = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date, kilometers });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ReadingDto>(Json))!;
    }

    private async Task<VehicleDto> GetAsync(Guid id)
    {
        var response = await _client.GetAsync(VehicleUri(id));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleDto>(Json))!;
    }

    private async Task<VehicleDto> FindInListAsync(VehicleDto vehicle)
    {
        var search = vehicle.LicensePlate.Replace("-", "", StringComparison.Ordinal);
        var response = await _client.GetAsync(new Uri($"/api/vehicles?search={search}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = (await response.Content.ReadFromJsonAsync<List<VehicleDto>>(Json))!;
        return list.Single(v => v.Id == vehicle.Id);
    }

    [Fact]
    public async Task Un_vehicule_sans_releve_n_a_pas_de_km_actuel()
    {
        var vehicle = await CreateVehicleAsync();

        Assert.Null(vehicle.CurrentMileage);
        var read = await GetAsync(vehicle.Id);
        var listed = await FindInListAsync(vehicle);
        Assert.Null(read.CurrentMileage);
        Assert.Null(read.CurrentMileageDate);
        Assert.Null(listed.CurrentMileage);
        Assert.Null(listed.CurrentMileageDate);
    }

    [Fact]
    public async Task Le_km_actuel_est_celui_du_releve_le_plus_recent_sur_la_fiche_et_la_liste()
    {
        var vehicle = await CreateVehicleAsync();
        await AddAsync(vehicle.Id, DaysAgo(10), 10_000);
        await AddAsync(vehicle.Id, DaysAgo(3), 10_300);

        var read = await GetAsync(vehicle.Id);
        var listed = await FindInListAsync(vehicle);

        Assert.Equal(10_300, read.CurrentMileage);
        Assert.Equal(DaysAgo(3), read.CurrentMileageDate);
        Assert.Equal(10_300, listed.CurrentMileage);
        Assert.Equal(DaysAgo(3), listed.CurrentMileageDate);
    }

    [Fact]
    public async Task A_date_egale_le_releve_le_plus_eleve_donne_le_km_actuel()
    {
        var vehicle = await CreateVehicleAsync();
        await AddAsync(vehicle.Id, DaysAgo(5), 20_050);
        await AddAsync(vehicle.Id, DaysAgo(5), 20_100);

        Assert.Equal(20_100, (await GetAsync(vehicle.Id)).CurrentMileage);
        Assert.Equal(20_100, (await FindInListAsync(vehicle)).CurrentMileage);
    }

    [Fact]
    public async Task Le_km_actuel_est_a_jour_apres_modification_et_suppression()
    {
        var vehicle = await CreateVehicleAsync();
        await AddAsync(vehicle.Id, DaysAgo(10), 5_000);
        var latest = await AddAsync(vehicle.Id, DaysAgo(2), 5_200);
        var readingUri = new Uri($"/api/vehicles/{vehicle.Id}/mileage-readings/{latest.Id}", UriKind.Relative);

        var update = await _client.PutAsJsonAsync(readingUri, new { date = DaysAgo(2), kilometers = 5_250 });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(5_250, (await GetAsync(vehicle.Id)).CurrentMileage);
        Assert.Equal(5_250, (await FindInListAsync(vehicle)).CurrentMileage);

        var delete = await _client.DeleteAsync(readingUri);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var after = await GetAsync(vehicle.Id);
        Assert.Equal(5_000, after.CurrentMileage);
        Assert.Equal(DaysAgo(10), after.CurrentMileageDate);
        Assert.Equal(5_000, (await FindInListAsync(vehicle)).CurrentMileage);
    }
}
