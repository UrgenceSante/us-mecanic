using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class DashboardEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public DashboardEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record VehicleLine(Guid Id, string LicensePlate, string? InternalNumber, Guid? TypeId, string? TypeName);

    private sealed record TypeLine(Guid Id, string Name);

    private sealed record Line(Guid Id, VehicleLine Vehicle, TypeLine DeadlineType, string Status, DateOnly? DueDate, int? DueKm, int? CurrentKm);

    /// <summary>Jour décalé par rapport à aujourd'hui (jamais 0, pour rester stable autour de minuit UTC).</summary>
    private static DateOnly Offset(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private async Task<Guid> PostIdAsync(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(new Uri(url, UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private Task<Guid> CreateVehicleAsync(Guid? typeId = null) =>
        PostIdAsync("/api/vehicles", new { licensePlate = NewPlate(), details = new { brand = "Renault", internalNumber = "AMB-7", typeId } });

    private Task<Guid> CreateTypeAsync() => PostIdAsync("/api/deadline-types", new { name = $"Type {Guid.NewGuid():N}" });

    private Task<Guid> AddDeadlineAsync(Guid vehicleId, Guid typeId, DateOnly dueDate) =>
        PostIdAsync($"/api/vehicles/{vehicleId}/deadlines", new { deadlineTypeId = typeId, dueDate });

    private async Task<List<Line>> ListAsync(string query)
    {
        var response = await _client.GetAsync(new Uri($"/api/dashboard/deadlines?{query}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<List<Line>>(Json))!;
    }

    [Fact]
    public async Task Le_tableau_de_bord_exige_une_authentification()
    {
        var response = await _anonymous.GetAsync(new Uri("/api/dashboard/deadlines", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Liste_triee_filtrable_et_sans_vehicule_sorti_du_parc()
    {
        var vehicleId = await CreateVehicleAsync();
        var retiredId = await CreateVehicleAsync();
        var typeId = await CreateTypeAsync();
        var otherTypeId = await CreateTypeAsync();
        var soon = await AddDeadlineAsync(vehicleId, typeId, Offset(5));
        var overdue = await AddDeadlineAsync(vehicleId, otherTypeId, Offset(-5));
        await AddDeadlineAsync(retiredId, typeId, Offset(-5));
        await AddDeadlineAsync(await CreateVehicleAsync(), typeId, Offset(300));
        var retire = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{retiredId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retire.StatusCode);

        var lines = await ListAsync($"vehicleId={vehicleId}");

        Assert.Equal([overdue, soon], lines.Select(l => l.Id));
        Assert.Equal(["Overdue", "DueSoon"], lines.Select(l => l.Status));
        Assert.Equal(vehicleId, lines[0].Vehicle.Id);
        Assert.Equal("AMB-7", lines[0].Vehicle.InternalNumber);
        Assert.Equal(otherTypeId, lines[0].DeadlineType.Id);
        Assert.Equal(Offset(-5), lines[0].DueDate);
        Assert.Null(lines[0].CurrentKm);

        Assert.Equal([soon], (await ListAsync($"vehicleId={vehicleId}&status=DueSoon")).Select(l => l.Id));
        Assert.Equal([overdue], (await ListAsync($"vehicleId={vehicleId}&deadlineTypeId={otherTypeId}")).Select(l => l.Id));
        Assert.Empty(await ListAsync($"vehicleId={retiredId}"));
        Assert.Empty(await ListAsync($"deadlineTypeId={typeId}&status=Overdue"));
    }

    [Fact]
    public async Task Filtre_par_type_de_vehicule_et_km_actuel()
    {
        var vehicleTypeId = await PostIdAsync("/api/vehicle-types", new { name = $"Type {Guid.NewGuid():N}" });
        var vehicleId = await CreateVehicleAsync(vehicleTypeId);
        var typeId = await CreateTypeAsync();
        var reading = await _client.PostAsJsonAsync(
            new Uri($"/api/vehicles/{vehicleId}/mileage-readings", UriKind.Relative), new { date = Offset(-2), kilometers = 49_500 });
        Assert.Equal(HttpStatusCode.Created, reading.StatusCode);
        await PostIdAsync($"/api/vehicles/{vehicleId}/deadlines", new { deadlineTypeId = typeId, dueKm = 50_000 });

        var line = Assert.Single(await ListAsync($"vehicleTypeId={vehicleTypeId}"));

        Assert.Equal("DueSoon", line.Status);
        Assert.Equal(50_000, line.DueKm);
        Assert.Equal(49_500, line.CurrentKm);
        Assert.Equal(vehicleTypeId, line.Vehicle.TypeId);
        Assert.StartsWith("Type ", line.Vehicle.TypeName, StringComparison.Ordinal);
    }
}
