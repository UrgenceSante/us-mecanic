using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleDeadlineEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public VehicleDeadlineEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record DeadlineDto(
        Guid Id,
        Guid VehicleId,
        Guid DeadlineTypeId,
        string? TypeName,
        int? IntervalMonths,
        int? IntervalKm,
        DateOnly? LastDoneDate,
        int? LastDoneKm,
        DateOnly? DueDate,
        int? DueKm,
        bool IsClosed,
        string? Status);

    private sealed record IdDto(Guid Id);

    /// <summary>Jour décalé par rapport à aujourd'hui (jamais 0, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly Offset(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri DeadlinesUri(Guid vehicleId) => new($"/api/vehicles/{vehicleId}/deadlines", UriKind.Relative);

    private static Uri IntervalUri(Guid vehicleId, Guid deadlineId) => new($"/api/vehicles/{vehicleId}/deadlines/{deadlineId}/interval", UriKind.Relative);

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

    private async Task<DeadlineDto> AddAsync(Guid vehicleId, object body)
    {
        var response = await _client.PostAsJsonAsync(DeadlinesUri(vehicleId), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DeadlineDto>(Json))!;
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task Les_echeances_exigent_une_authentification()
    {
        var response = await _anonymous.GetAsync(DeadlinesUri(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Ajout_ponctuelle_puis_liste_avec_statut()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();

        var response = await _client.PostAsJsonAsync(DeadlinesUri(vehicleId), new { deadlineTypeId = typeId, dueDate = Offset(10) });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<DeadlineDto>(Json))!;
        Assert.Equal($"/api/vehicles/{vehicleId}/deadlines/{created.Id}", response.Headers.Location?.ToString());
        Assert.Equal(vehicleId, created.VehicleId);
        Assert.Equal(Offset(10), created.DueDate);
        Assert.Equal("DueSoon", created.Status);
        Assert.False(created.IsClosed);

        var list = await _client.GetFromJsonAsync<List<DeadlineDto>>(DeadlinesUri(vehicleId), Json);
        Assert.Single(list!);
        Assert.StartsWith("Type ", list![0].TypeName, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Ajout_periodique_calcule_la_prochaine_echeance_et_le_statut_depuis_le_km_actuel()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var reading = await _client.PostAsJsonAsync(
            new Uri($"/api/vehicles/{vehicleId}/mileage-readings", UriKind.Relative), new { date = Offset(-2), kilometers = 49_500 });
        Assert.Equal(HttpStatusCode.Created, reading.StatusCode);

        var created = await AddAsync(vehicleId, new
        {
            deadlineTypeId = typeId,
            intervalMonths = 12,
            intervalKm = 30_000,
            lastDoneDate = Offset(-60),
            lastDoneKm = 20_000,
        });

        Assert.Equal(Offset(-60).AddMonths(12), created.DueDate);
        Assert.Equal(50_000, created.DueKm);
        Assert.Equal(12, created.IntervalMonths);
        Assert.Equal("DueSoon", created.Status); // 500 km restants
    }

    [Fact]
    public async Task Une_echeance_depassee_est_signalee()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();

        var created = await AddAsync(vehicleId, new { deadlineTypeId = typeId, dueDate = Offset(-3) });

        Assert.Equal("Overdue", created.Status);
    }

    [Fact]
    public async Task Un_meme_type_ne_peut_etre_actif_qu_une_fois_par_vehicule()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        await AddAsync(vehicleId, new { deadlineTypeId = typeId, dueDate = Offset(100) });

        var again = await _client.PostAsJsonAsync(DeadlinesUri(vehicleId), new { deadlineTypeId = typeId, dueDate = Offset(200) });

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("vehicle_deadline.already_active", await ErrorCodeAsync(again));
    }

    [Fact]
    public async Task Sans_critere_ou_avec_un_type_inconnu_ou_inactif_l_ajout_est_refuse()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var inactiveId = await CreateTypeAsync();
        var deactivated = await _client.PostAsync(new Uri($"/api/deadline-types/{inactiveId}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);

        var noCriterion = await _client.PostAsJsonAsync(DeadlinesUri(vehicleId), new { deadlineTypeId = typeId });
        var unknown = await _client.PostAsJsonAsync(DeadlinesUri(vehicleId), new { deadlineTypeId = Guid.NewGuid(), dueDate = Offset(5) });
        var inactive = await _client.PostAsJsonAsync(DeadlinesUri(vehicleId), new { deadlineTypeId = inactiveId, dueDate = Offset(5) });

        Assert.Equal(HttpStatusCode.BadRequest, noCriterion.StatusCode);
        Assert.Equal("vehicle_deadline.criterion_required", await ErrorCodeAsync(noCriterion));
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal("vehicle_deadline.type_unknown", await ErrorCodeAsync(unknown));
        Assert.Equal(HttpStatusCode.BadRequest, inactive.StatusCode);
        Assert.Equal("vehicle_deadline.type_inactive", await ErrorCodeAsync(inactive));
    }

    [Fact]
    public async Task Modification_de_la_periodicite_recalcule_la_prochaine_echeance()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var created = await AddAsync(vehicleId, new { deadlineTypeId = typeId, intervalMonths = 12, lastDoneDate = Offset(-30) });

        var response = await _client.PutAsJsonAsync(IntervalUri(vehicleId, created.Id), new { intervalMonths = 6 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<DeadlineDto>(Json))!;
        Assert.Equal(6, updated.IntervalMonths);
        Assert.Equal(Offset(-30).AddMonths(6), updated.DueDate);
    }

    [Fact]
    public async Task Modification_d_une_periodicite_invalide_ou_d_une_echeance_inconnue()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var created = await AddAsync(vehicleId, new { deadlineTypeId = typeId, dueDate = Offset(100) });

        var invalid = await _client.PutAsJsonAsync(IntervalUri(vehicleId, created.Id), new { intervalMonths = 0 });
        var unknown = await _client.PutAsJsonAsync(IntervalUri(vehicleId, Guid.NewGuid()), new { intervalMonths = 6 });

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("vehicle_deadline.interval_months_invalid", await ErrorCodeAsync(invalid));
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("vehicle_deadline.not_found", await ErrorCodeAsync(unknown));
    }

    [Fact]
    public async Task Un_vehicule_inconnu_renvoie_404()
    {
        var unknown = Guid.NewGuid();

        var list = await _client.GetAsync(DeadlinesUri(unknown));
        var add = await _client.PostAsJsonAsync(DeadlinesUri(unknown), new { deadlineTypeId = Guid.NewGuid(), dueDate = Offset(5) });

        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(list));
        Assert.Equal(HttpStatusCode.NotFound, add.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(add));
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_refuse_l_ecriture_mais_reste_consultable()
    {
        var vehicleId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var created = await AddAsync(vehicleId, new { deadlineTypeId = typeId, dueDate = Offset(100) });
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        var add = await _client.PostAsJsonAsync(DeadlinesUri(vehicleId), new { deadlineTypeId = await CreateTypeAsync(), dueDate = Offset(5) });
        var update = await _client.PutAsJsonAsync(IntervalUri(vehicleId, created.Id), new { intervalMonths = 6 });
        var list = await _client.GetAsync(DeadlinesUri(vehicleId));

        Assert.Equal(HttpStatusCode.Conflict, add.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(add));
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(update));
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
    }
}
