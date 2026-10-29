using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class InterventionEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public InterventionEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record GarageDto(Guid Id, string Name);

    private sealed record DoneDeadlineDto(Guid Id, string? TypeName);

    private sealed record InterventionDto(
        Guid Id, Guid VehicleId, DateOnly Date, int? Kilometers, GarageDto? Garage, string Work, List<DoneDeadlineDto> Deadlines);

    private sealed record DeadlineDto(Guid Id, DateOnly? LastDoneDate, int? LastDoneKm, DateOnly? DueDate, int? DueKm, bool IsClosed);

    private sealed record ReadingDto(Guid Id, DateOnly Date, int Kilometers);

    /// <summary>Jour décalé par rapport à aujourd'hui (jamais 0, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly Offset(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri InterventionsUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/interventions", UriKind.Relative);

    private static Uri ReadingsUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/mileage-readings", UriKind.Relative);

    private async Task<Guid> CreateVehicleAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = NewPlate(), details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<Guid> CreateTypeAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/deadline-types", UriKind.Relative), new { name = $"{name} {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<GarageDto> CreateGarageAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/garages", UriKind.Relative), new { name = $"Garage {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<GarageDto>(Json))!;
    }

    private async Task<DeadlineDto> AddDeadlineAsync(Guid vehicleId, object body)
    {
        var response = await _client.PostAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/deadlines", UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DeadlineDto>(Json))!;
    }

    private async Task<List<DeadlineDto>> ListDeadlinesAsync(Guid vehicleId) =>
        (await _client.GetFromJsonAsync<List<DeadlineDto>>(new Uri($"/api/vehicles/{vehicleId}/deadlines", UriKind.Relative), Json))!;

    private async Task<List<ReadingDto>> ListReadingsAsync(Guid vehicleId) =>
        (await _client.GetFromJsonAsync<List<ReadingDto>>(ReadingsUri(vehicleId), Json))!;

    private static Uri InterventionUri(Guid vehicleId, Guid interventionId) =>
        new($"/api/vehicles/{vehicleId}/interventions/{interventionId}", UriKind.Relative);

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Le_carnet_exige_une_authentification()
    {
        var read = await _anonymous.GetAsync(InterventionsUri(Guid.NewGuid()));
        var write = await _anonymous.PostAsJsonAsync(InterventionsUri(Guid.NewGuid()), new { date = Offset(-1), work = "Vidange" });

        Assert.Equal(HttpStatusCode.Unauthorized, read.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
    }

    [Fact]
    public async Task Une_intervention_realise_les_echeances_cochees_cree_le_releve_et_apparait_dans_le_carnet()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync("Entretien");
        var garage = await CreateGarageAsync();
        var deadline = await AddDeadlineAsync(vehicleId, new
        {
            deadlineTypeId = typeId,
            intervalMonths = 12,
            intervalKm = 30_000,
            lastDoneDate = Offset(-300),
            lastDoneKm = 20_000,
        });
        var date = Offset(-3);

        var response = await _client.PostAsJsonAsync(
            InterventionsUri(vehicleId),
            new { date, kilometers = 21_000, garageId = garage.Id, work = "Vidange complète", deadlineIds = new[] { deadline.Id } });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<InterventionDto>(Json))!;
        Assert.Equal($"/api/vehicles/{vehicleId}/interventions/{created.Id}", response.Headers.Location?.ToString());
        Assert.Equal(garage, created.Garage);
        Assert.Equal(deadline.Id, Assert.Single(created.Deadlines).Id);

        var updated = Assert.Single(await ListDeadlinesAsync(vehicleId));
        Assert.Equal(date, updated.LastDoneDate);
        Assert.Equal(21_000, updated.LastDoneKm);
        Assert.Equal(date.AddMonths(12), updated.DueDate);
        Assert.Equal(51_000, updated.DueKm);
        var reading = Assert.Single(await ListReadingsAsync(vehicleId));
        Assert.Equal(date, reading.Date);
        Assert.Equal(21_000, reading.Kilometers);

        var logbook = await _client.GetFromJsonAsync<List<InterventionDto>>(InterventionsUri(vehicleId), Json);
        var entry = Assert.Single(logbook!);
        Assert.Equal(created.Id, entry.Id);
        Assert.Equal("Vidange complète", entry.Work);
        Assert.StartsWith("Entretien ", Assert.Single(entry.Deadlines).TypeName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Le_carnet_liste_la_plus_recente_en_premier()
    {
        var vehicleId = await CreateVehicleAsync();
        foreach (var (days, work) in new[] { (-10, "Pneus"), (-2, "Freins"), (-6, "Batterie") })
        {
            var response = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(days), work });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var logbook = await _client.GetFromJsonAsync<List<InterventionDto>>(InterventionsUri(vehicleId), Json);

        string[] expected = ["Freins", "Batterie", "Pneus"];
        Assert.Equal(expected, logbook!.Select(i => i.Work).ToArray());
        Assert.All(logbook!, i => Assert.Empty(i.Deadlines));
        Assert.All(logbook!, i => Assert.Null(i.Garage));
    }

    [Fact]
    public async Task Un_km_obligatoire_manquant_refuse_l_intervention_et_rien_n_est_enregistre()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync("Entretien");
        var deadline = await AddDeadlineAsync(vehicleId, new { deadlineTypeId = typeId, dueKm = 50_000 });

        var response = await _client.PostAsJsonAsync(
            InterventionsUri(vehicleId), new { date = Offset(-1), work = "Vidange", deadlineIds = new[] { deadline.Id } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("deadline_completion.kilometers_required", await ErrorCodeAsync(response));
        Assert.False(Assert.Single(await ListDeadlinesAsync(vehicleId)).IsClosed);
        Assert.Empty((await _client.GetFromJsonAsync<List<InterventionDto>>(InterventionsUri(vehicleId), Json))!);
    }

    [Fact]
    public async Task Un_releve_incoherent_refuse_tout_sans_ecrire_puis_passe_avec_un_motif()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync("Entretien");
        var seed = await _client.PostAsJsonAsync(ReadingsUri(vehicleId), new { date = Offset(-30), kilometers = 30_000 });
        Assert.Equal(HttpStatusCode.Created, seed.StatusCode);
        var deadline = await AddDeadlineAsync(vehicleId, new { deadlineTypeId = typeId, intervalMonths = 12, lastDoneDate = Offset(-100) });
        var body = new { date = Offset(-5), kilometers = 25_000, work = "Vidange", deadlineIds = new[] { deadline.Id } };

        var refused = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), body);

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Equal("mileage_reading.reason_required", await ErrorCodeAsync(refused));
        Assert.Single(await ListReadingsAsync(vehicleId));
        Assert.Equal(Offset(-100), Assert.Single(await ListDeadlinesAsync(vehicleId)).LastDoneDate);

        var accepted = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { body.date, body.kilometers, body.work, body.deadlineIds, reason = "Compteur remplacé" });
        Assert.Equal(HttpStatusCode.Created, accepted.StatusCode);
        Assert.Equal(2, (await ListReadingsAsync(vehicleId)).Count);
    }

    [Fact]
    public async Task Echeances_et_garage_invalides_sont_refuses()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherVehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync("Contrôle");
        var deadline = await AddDeadlineAsync(vehicleId, new { deadlineTypeId = typeId, dueDate = Offset(10) });
        var foreign = await AddDeadlineAsync(otherVehicleId, new { deadlineTypeId = typeId, dueDate = Offset(10) });
        var inactive = await CreateGarageAsync();
        var deactivated = await _client.PostAsync(new Uri($"/api/garages/{inactive.Id}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);

        var duplicate = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Contrôle", deadlineIds = new[] { deadline.Id, deadline.Id } });
        var otherVehicle = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Contrôle", deadlineIds = new[] { foreign.Id } });
        var unknownGarage = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Contrôle", garageId = Guid.NewGuid() });
        var inactiveGarage = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Contrôle", garageId = inactive.Id });
        var done = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Contrôle", deadlineIds = new[] { deadline.Id } });
        var closed = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Contrôle bis", deadlineIds = new[] { deadline.Id } });

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal("intervention.deadline_duplicated", await ErrorCodeAsync(duplicate));
        Assert.Equal(HttpStatusCode.BadRequest, otherVehicle.StatusCode);
        Assert.Equal("intervention.deadline_unknown", await ErrorCodeAsync(otherVehicle));
        Assert.Equal(HttpStatusCode.BadRequest, unknownGarage.StatusCode);
        Assert.Equal("intervention.garage_unknown", await ErrorCodeAsync(unknownGarage));
        Assert.Equal(HttpStatusCode.BadRequest, inactiveGarage.StatusCode);
        Assert.Equal("intervention.garage_inactive", await ErrorCodeAsync(inactiveGarage));
        Assert.Equal(HttpStatusCode.Created, done.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, closed.StatusCode);
        Assert.Equal("intervention.deadline_closed", await ErrorCodeAsync(closed));
        Assert.Single((await _client.GetFromJsonAsync<List<InterventionDto>>(InterventionsUri(vehicleId), Json))!);
    }

    [Fact]
    public async Task Travaux_date_future_vehicule_inconnu_et_vehicule_sorti_du_parc_sont_refuses()
    {
        var vehicleId = await CreateVehicleAsync();

        var noWork = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = " " });
        var future = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(5), work = "Vidange" });
        var unknown = await _client.PostAsJsonAsync(InterventionsUri(Guid.NewGuid()), new { date = Offset(-1), work = "Vidange" });
        var unknownRead = await _client.GetAsync(InterventionsUri(Guid.NewGuid()));
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var afterRetired = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Vidange" });
        var readRetired = await _client.GetAsync(InterventionsUri(vehicleId));

        Assert.Equal(HttpStatusCode.BadRequest, noWork.StatusCode);
        Assert.Equal("intervention.work_required", await ErrorCodeAsync(noWork));
        Assert.Equal(HttpStatusCode.BadRequest, future.StatusCode);
        Assert.Equal("intervention.date_in_future", await ErrorCodeAsync(future));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknownRead.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, afterRetired.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(afterRetired));
        Assert.Equal(HttpStatusCode.OK, readRetired.StatusCode);
    }

    [Fact]
    public async Task La_suppression_retire_l_intervention_sans_recalculer_l_echeance_et_conserve_le_releve()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync("Entretien");
        var deadline = await AddDeadlineAsync(vehicleId, new
        {
            deadlineTypeId = typeId,
            intervalMonths = 12,
            intervalKm = 30_000,
            lastDoneDate = Offset(-300),
            lastDoneKm = 20_000,
        });
        var date = Offset(-3);
        var created = await _client.PostAsJsonAsync(
            InterventionsUri(vehicleId), new { date, kilometers = 21_000, work = "Vidange", deadlineIds = new[] { deadline.Id } });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var intervention = (await created.Content.ReadFromJsonAsync<InterventionDto>(Json))!;

        var deleted = await _client.DeleteAsync(InterventionUri(vehicleId, intervention.Id));
        var again = await _client.DeleteAsync(InterventionUri(vehicleId, intervention.Id));
        var logbook = await _client.GetFromJsonAsync<List<InterventionDto>>(InterventionsUri(vehicleId), Json);
        var deadlines = await ListDeadlinesAsync(vehicleId);
        var readings = await ListReadingsAsync(vehicleId);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
        Assert.Equal("intervention.not_found", await ErrorCodeAsync(again));
        Assert.Empty(logbook!);
        var kept = Assert.Single(deadlines);
        Assert.Equal(date, kept.LastDoneDate);
        Assert.Equal(21_000, kept.LastDoneKm);
        Assert.Equal(date.AddMonths(12), kept.DueDate);
        Assert.Equal(51_000, kept.DueKm);
        Assert.Equal(21_000, Assert.Single(readings).Kilometers);
    }

    [Fact]
    public async Task La_suppression_d_une_intervention_liee_a_plusieurs_echeances_reussit_sans_les_recalculer()
    {
        var vehicleId = await CreateVehicleAsync();
        var first = await AddDeadlineAsync(vehicleId, new
        {
            deadlineTypeId = await CreateTypeAsync("Vidange"),
            intervalMonths = 12,
            intervalKm = 30_000,
            lastDoneDate = Offset(-300),
            lastDoneKm = 20_000,
        });
        var second = await AddDeadlineAsync(vehicleId, new
        {
            deadlineTypeId = await CreateTypeAsync("Freins"),
            intervalMonths = 24,
            intervalKm = 60_000,
            lastDoneDate = Offset(-200),
            lastDoneKm = 10_000,
        });
        var created = await _client.PostAsJsonAsync(
            InterventionsUri(vehicleId),
            new { date = Offset(-3), kilometers = 21_000, work = "Entretien complet", deadlineIds = new[] { first.Id, second.Id } });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var intervention = (await created.Content.ReadFromJsonAsync<InterventionDto>(Json))!;
        Assert.Equal(2, intervention.Deadlines.Count);
        var beforeDelete = (await ListDeadlinesAsync(vehicleId)).OrderBy(d => d.Id).ToList();

        var deleted = await _client.DeleteAsync(InterventionUri(vehicleId, intervention.Id));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        var logbook = await _client.GetFromJsonAsync<List<InterventionDto>>(InterventionsUri(vehicleId), Json);
        var afterDelete = (await ListDeadlinesAsync(vehicleId)).OrderBy(d => d.Id).ToList();

        Assert.Empty(logbook!);
        Assert.Equal(2, afterDelete.Count);
        Assert.Equal(beforeDelete, afterDelete);
    }

    [Fact]
    public async Task La_suppression_refuse_une_intervention_d_un_autre_vehicule_ou_un_vehicule_sorti_du_parc()
    {
        var vehicleId = await CreateVehicleAsync();
        var otherId = await CreateVehicleAsync();
        var created = await _client.PostAsJsonAsync(InterventionsUri(vehicleId), new { date = Offset(-1), work = "Vidange" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var intervention = (await created.Content.ReadFromJsonAsync<InterventionDto>(Json))!;

        var otherVehicle = await _client.DeleteAsync(InterventionUri(otherId, intervention.Id));
        var unknownVehicle = await _client.DeleteAsync(InterventionUri(Guid.NewGuid(), intervention.Id));
        var anonymous = await _anonymous.DeleteAsync(InterventionUri(vehicleId, intervention.Id));
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);
        var afterRetired = await _client.DeleteAsync(InterventionUri(vehicleId, intervention.Id));

        Assert.Equal(HttpStatusCode.NotFound, otherVehicle.StatusCode);
        Assert.Equal("intervention.not_found", await ErrorCodeAsync(otherVehicle));
        Assert.Equal(HttpStatusCode.NotFound, unknownVehicle.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, afterRetired.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(afterRetired));
        Assert.Single((await _client.GetFromJsonAsync<List<InterventionDto>>(InterventionsUri(vehicleId), Json))!);
    }
}
