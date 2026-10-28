using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class DeadlineTypeAssignEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public DeadlineTypeAssignEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record ErrorDto(Guid VehicleId, string LicensePlate, string Code, string Message);

    private sealed record ReportDto(int Added, int SkippedAlreadyActive, int SkippedRetired, List<ErrorDto> Errors);

    private sealed record DeadlineDto(Guid DeadlineTypeId, int? IntervalMonths, DateOnly? LastDoneDate, DateOnly? DueDate);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri AssignUri(Guid typeId) => new($"/api/deadline-types/{typeId}/assign", UriKind.Relative);

    private async Task<Guid> PostIdAsync(string uri, object body)
    {
        var response = await _client.PostAsJsonAsync(new Uri(uri, UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private Task<Guid> CreateVehicleTypeAsync() => PostIdAsync("/api/vehicle-types", new { name = $"VT {Guid.NewGuid():N}" });

    private Task<Guid> CreateVehicleAsync(Guid vehicleTypeId) =>
        PostIdAsync("/api/vehicles", new { licensePlate = NewPlate(), details = new { brand = "Renault", typeId = vehicleTypeId } });

    private Task<Guid> CreateDeadlineTypeAsync(object? interval = null) =>
        PostIdAsync("/api/deadline-types", new { name = $"DT {Guid.NewGuid():N}", defaultIntervalMonths = interval });

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task L_ajout_en_lot_exige_une_authentification()
    {
        var response = await _anonymous.PostAsJsonAsync(AssignUri(Guid.NewGuid()), new { vehicleTypeIds = new[] { Guid.NewGuid() } });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Ajoute_aux_vehicules_actifs_ignore_les_sortis_et_les_deja_concernes()
    {
        var vehicleTypeId = await CreateVehicleTypeAsync();
        var vehicleId = await CreateVehicleAsync(vehicleTypeId);
        var existingId = await CreateVehicleAsync(vehicleTypeId);
        var retiredId = await CreateVehicleAsync(vehicleTypeId);
        var typeId = await CreateDeadlineTypeAsync(12);
        await PostIdAsync($"/api/vehicles/{existingId}/deadlines", new { deadlineTypeId = typeId, intervalMonths = 6, lastDoneDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-3) });
        var retire = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{retiredId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retire.StatusCode);
        var done = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);

        var response = await _client.PostAsJsonAsync(AssignUri(typeId), new { vehicleTypeIds = new[] { vehicleTypeId }, lastDoneDate = done });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = (await response.Content.ReadFromJsonAsync<ReportDto>(Json))!;
        Assert.Equal(1, report.Added);
        Assert.Equal(1, report.SkippedAlreadyActive);
        Assert.Equal(1, report.SkippedRetired);
        Assert.Empty(report.Errors);
        var deadlines = await _client.GetFromJsonAsync<List<DeadlineDto>>(new Uri($"/api/vehicles/{vehicleId}/deadlines", UriKind.Relative), Json);
        var created = Assert.Single(deadlines!);
        Assert.Equal(typeId, created.DeadlineTypeId);
        Assert.Equal(12, created.IntervalMonths);
        Assert.Equal(done, created.LastDoneDate);
        Assert.Equal(done.AddMonths(12), created.DueDate);

        var again = await _client.PostAsJsonAsync(AssignUri(typeId), new { vehicleTypeIds = new[] { vehicleTypeId } });
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        var second = (await again.Content.ReadFromJsonAsync<ReportDto>(Json))!;
        Assert.Equal(0, second.Added);
        Assert.Equal(2, second.SkippedAlreadyActive);
    }

    [Fact]
    public async Task Refuse_un_type_ponctuel_sans_vehicule_et_un_type_de_vehicule_inconnu()
    {
        var oneOff = await CreateDeadlineTypeAsync();
        var periodic = await CreateDeadlineTypeAsync(12);

        var oneOffResponse = await _client.PostAsJsonAsync(AssignUri(oneOff), new { vehicleTypeIds = new[] { Guid.NewGuid() } });
        Assert.Equal(HttpStatusCode.BadRequest, oneOffResponse.StatusCode);
        Assert.Equal("vehicle_deadline.one_off_not_assignable", await ErrorCodeAsync(oneOffResponse));

        var empty = await _client.PostAsJsonAsync(AssignUri(periodic), new { vehicleTypeIds = Array.Empty<Guid>() });
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal("vehicle_deadline.vehicle_types_required", await ErrorCodeAsync(empty));

        var unknown = await _client.PostAsJsonAsync(AssignUri(periodic), new { vehicleTypeIds = new[] { Guid.NewGuid() } });
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal("vehicle_type.unknown", await ErrorCodeAsync(unknown));

        var missing = await _client.PostAsJsonAsync(AssignUri(Guid.NewGuid()), new { vehicleTypeIds = new[] { Guid.NewGuid() } });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("deadline_type.not_found", await ErrorCodeAsync(missing));
    }

    [Fact]
    public async Task Refuse_un_type_d_echeance_desactive()
    {
        var vehicleTypeId = await CreateVehicleTypeAsync();
        var typeId = await CreateDeadlineTypeAsync(12);
        var deactivate = await _client.PostAsync(new Uri($"/api/deadline-types/{typeId}/deactivate", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);

        var response = await _client.PostAsJsonAsync(AssignUri(typeId), new { vehicleTypeIds = new[] { vehicleTypeId } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("vehicle_deadline.type_inactive", await ErrorCodeAsync(response));
    }
}
