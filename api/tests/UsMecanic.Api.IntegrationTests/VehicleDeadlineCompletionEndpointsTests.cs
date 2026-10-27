using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleDeadlineCompletionEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public VehicleDeadlineCompletionEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record DeadlineDto(
        Guid Id,
        Guid VehicleId,
        DateOnly? LastDoneDate,
        int? LastDoneKm,
        DateOnly? DueDate,
        int? DueKm,
        bool IsClosed,
        string? Status);

    private sealed record ReadingDto(Guid Id, DateOnly Date, int Kilometers, string? Reason);

    private sealed record IdDto(Guid Id);

    /// <summary>Jour décalé par rapport à aujourd'hui (jamais 0, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly Offset(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri CompletionsUri(Guid vehicleId, Guid deadlineId) =>
        new($"/api/vehicles/{vehicleId}/deadlines/{deadlineId}/completions", UriKind.Relative);

    private static Uri ReadingsUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/mileage-readings", UriKind.Relative);

    private async Task<Guid> CreateVehicleAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = NewPlate(), details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<Guid> CreateTypeAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/deadline-types", UriKind.Relative), new { name = $"Type {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<DeadlineDto> AddDeadlineAsync(Guid vehicleId, object body)
    {
        var response = await _client.PostAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/deadlines", UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DeadlineDto>(Json))!;
    }

    private async Task AddReadingAsync(Guid vehicleId, DateOnly date, int kilometers)
    {
        var response = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date, kilometers });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task La_realisation_exige_une_authentification()
    {
        var response = await _anonymous.PostAsJsonAsync(CompletionsUri(Guid.NewGuid(), Guid.NewGuid()), new { date = Offset(-1) });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Periodique_realisee_recalcule_la_suivante_depuis_la_date_reelle_et_cree_le_releve()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var deadline = await AddDeadlineAsync(vehicleId, new
        {
            deadlineTypeId = typeId,
            intervalMonths = 12,
            intervalKm = 30_000,
            lastDoneDate = Offset(-300),
            lastDoneKm = 20_000,
        });
        var date = Offset(-3);

        var response = await _client.PostAsJsonAsync(CompletionsUri(vehicleId, deadline.Id), new { date, kilometers = 21_000, comment = "Révision" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<DeadlineDto>(Json))!;
        Assert.Equal(date, updated.LastDoneDate);
        Assert.Equal(21_000, updated.LastDoneKm);
        Assert.Equal(date.AddMonths(12), updated.DueDate);
        Assert.Equal(51_000, updated.DueKm);
        Assert.False(updated.IsClosed);
        Assert.Equal("UpToDate", updated.Status);

        var readings = await _client.GetFromJsonAsync<List<ReadingDto>>(ReadingsUri(vehicleId), Json);
        var reading = Assert.Single(readings!);
        Assert.Equal((date, 21_000), (reading.Date, reading.Kilometers));
    }

    [Fact]
    public async Task Ponctuelle_realisee_est_close_sans_statut_et_ne_peut_plus_etre_realisee()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var deadline = await AddDeadlineAsync(vehicleId, new { deadlineTypeId = typeId, dueDate = Offset(10) });

        var response = await _client.PostAsJsonAsync(CompletionsUri(vehicleId, deadline.Id), new { date = Offset(-1) });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<DeadlineDto>(Json))!;
        Assert.True(updated.IsClosed);
        Assert.Null(updated.Status);
        Assert.Equal(Offset(-1), updated.LastDoneDate);

        var again = await _client.PostAsJsonAsync(CompletionsUri(vehicleId, deadline.Id), new { date = Offset(-1) });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("vehicle_deadline.closed", await ErrorCodeAsync(again));
    }

    [Fact]
    public async Task Un_km_incoherent_sans_motif_est_refuse_et_rien_n_est_enregistre()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        await AddReadingAsync(vehicleId, Offset(-30), 30_000);
        var deadline = await AddDeadlineAsync(vehicleId, new { deadlineTypeId = typeId, intervalMonths = 12, lastDoneDate = Offset(-100) });

        var refused = await _client.PostAsJsonAsync(CompletionsUri(vehicleId, deadline.Id), new { date = Offset(-5), kilometers = 25_000 });

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("mileage_reading.reason_required", await ErrorCodeAsync(refused));
        var readings = await _client.GetFromJsonAsync<List<ReadingDto>>(ReadingsUri(vehicleId), Json);
        Assert.Single(readings!);

        var accepted = await _client.PostAsJsonAsync(
            CompletionsUri(vehicleId, deadline.Id), new { date = Offset(-5), kilometers = 25_000, reason = "Compteur remplacé" });
        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        readings = await _client.GetFromJsonAsync<List<ReadingDto>>(ReadingsUri(vehicleId), Json);
        Assert.Equal(2, readings!.Count);
        Assert.Contains(readings, r => r.Kilometers == 25_000 && r.Reason == "Compteur remplacé");
    }

    [Fact]
    public async Task Date_future_echeance_inconnue_et_vehicule_sorti_du_parc_sont_refuses()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var deadline = await AddDeadlineAsync(vehicleId, new { deadlineTypeId = typeId, dueDate = Offset(10) });

        var future = await _client.PostAsJsonAsync(CompletionsUri(vehicleId, deadline.Id), new { date = Offset(5) });
        var unknown = await _client.PostAsJsonAsync(CompletionsUri(vehicleId, Guid.NewGuid()), new { date = Offset(-1) });
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var afterRetired = await _client.PostAsJsonAsync(CompletionsUri(vehicleId, deadline.Id), new { date = Offset(-1) });

        Assert.Equal(HttpStatusCode.BadRequest, future.StatusCode);
        Assert.Equal("deadline_completion.date_in_future", await ErrorCodeAsync(future));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("vehicle_deadline.not_found", await ErrorCodeAsync(unknown));
        Assert.Equal(HttpStatusCode.Conflict, afterRetired.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(afterRetired));
    }
}
