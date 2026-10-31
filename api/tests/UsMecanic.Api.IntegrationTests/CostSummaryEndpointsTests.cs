using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class CostSummaryEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public CostSummaryEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record FamilyDto(string Family, decimal Total);

    private sealed record CategoryTotalDto(string Family, string Category, decimal Total);

    private sealed record PeriodDto(string Period, DateOnly Start, decimal Total, List<FamilyDto> Families, List<CategoryTotalDto> Categories);

    private sealed record CostPerKmDto(int Kilometers, decimal Cost, decimal? Value, bool Computable, int ExcludedVehicles);

    private sealed record SummaryDto(
        DateOnly From, DateOnly To, string Granularity, Guid? VehicleId, decimal Total, List<FamilyDto> Families, List<CategoryTotalDto> Categories, List<PeriodDto> Periods, CostPerKmDto CostPerKm);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static Uri SummaryUri(string query) => new($"/api/costs/summary?{query}", UriKind.Relative);

    private async Task<Guid> CreateVehicleAsync()
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = NewPlate(), details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task PostAsync(string uri, object body)
    {
        var response = await _client.PostAsJsonAsync(new Uri(uri, UriKind.Relative), body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task La_synthese_exige_une_authentification()
    {
        var response = await _anonymous.GetAsync(SummaryUri("from=2026-01-01&to=2026-12-31"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Synthese_d_un_vehicule_avec_intervention_depense_et_emprunt()
    {
        var vehicleId = await CreateVehicleAsync();
        var category = await _client.PostAsJsonAsync(new Uri("/api/expense-categories", UriKind.Relative), new { name = $"Péage {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, category.StatusCode);
        var categoryDto = (await category.Content.ReadFromJsonAsync<IdDto>(Json))!;
        await PostAsync($"/api/vehicles/{vehicleId}/interventions", new { date = "2025-03-15", work = "Vidange", amount = 120m });
        await PostAsync($"/api/vehicles/{vehicleId}/expenses", new { date = "2025-03-20", categoryId = categoryDto.Id, amount = 30.5m });
        await PostAsync($"/api/vehicles/{vehicleId}/loans", new { lender = "Banque", borrowedAmount = 3600m, monthlyPayment = 300m, startDate = "2025-01-10", durationMonths = 12 });

        var response = await _client.GetAsync(SummaryUri($"from=2025-01-01&to=2025-12-31&granularity=month&vehicleId={vehicleId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = (await response.Content.ReadFromJsonAsync<SummaryDto>(Json))!;
        Assert.Equal("Month", summary.Granularity);
        Assert.Equal(vehicleId, summary.VehicleId);
        Assert.Equal(12, summary.Periods.Count);
        Assert.Equal(3750.5m, summary.Total);
        Assert.Equal(3600m, summary.Families.Single(f => f.Family == "Loan").Total);
        Assert.Equal(150.5m, summary.Families.Single(f => f.Family == "OneOff").Total);
        Assert.Equal(450.5m, summary.Periods.Single(p => p.Period == "2025-03").Total);
        Assert.Equal(300m, summary.Periods.Single(p => p.Period == "2025-04").Total);
        Assert.Contains(summary.Categories, c => c.Category == "Entretien et réparation" && c.Total == 120m);
    }

    [Fact]
    public async Task Synthese_annuelle_du_parc()
    {
        var vehicleId = await CreateVehicleAsync();
        await PostAsync($"/api/vehicles/{vehicleId}/loans", new { lender = "Banque", borrowedAmount = 1000m, monthlyPayment = 100m, startDate = "2031-11-01", durationMonths = 4 });

        var response = await _client.GetAsync(SummaryUri("from=2031-01-01&to=2032-12-31&granularity=year"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = (await response.Content.ReadFromJsonAsync<SummaryDto>(Json))!;
        Assert.Equal(["2031", "2032"], summary.Periods.Select(p => p.Period));
        Assert.True(summary.Periods[0].Total >= 200m);
        Assert.True(summary.Periods[1].Total >= 200m);
    }

    [Fact]
    public async Task Vehicule_inconnu_renvoie_404()
    {
        var response = await _client.GetAsync(SummaryUri($"from=2026-01-01&to=2026-12-31&vehicleId={Guid.NewGuid()}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("vehicle.not_found", await ErrorCodeAsync(response));
    }

    [Theory]
    [InlineData("to=2026-12-31", "cost_summary.from_required")]
    [InlineData("from=2026-01-01", "cost_summary.to_required")]
    [InlineData("from=2026-12-31&to=2026-01-01", "cost_summary.invalid_range")]
    [InlineData("from=2026-01-01&to=2026-12-31&granularity=week", "cost_summary.invalid_granularity")]
    [InlineData("from=2000-01-01&to=2026-12-31", "cost_summary.range_too_long")]
    public async Task Parametres_invalides_renvoient_400(string query, string code)
    {
        var response = await _client.GetAsync(SummaryUri(query));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(code, await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Cout_au_km_d_un_vehicule_avec_deux_releves_dans_la_periode()
    {
        var vehicleId = await CreateVehicleAsync();
        await PostAsync($"/api/vehicles/{vehicleId}/mileage-readings", new { date = "2025-01-10", kilometers = 5000 });
        await PostAsync($"/api/vehicles/{vehicleId}/mileage-readings", new { date = "2025-06-10", kilometers = 6000 });
        await PostAsync($"/api/vehicles/{vehicleId}/interventions", new { date = "2025-03-15", work = "Vidange", amount = 250m });

        var response = await _client.GetAsync(SummaryUri($"from=2025-01-01&to=2025-12-31&vehicleId={vehicleId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var costPerKm = (await response.Content.ReadFromJsonAsync<SummaryDto>(Json))!.CostPerKm;
        Assert.True(costPerKm.Computable);
        Assert.Equal(1000, costPerKm.Kilometers);
        Assert.Equal(250m, costPerKm.Cost);
        Assert.Equal(0.25m, costPerKm.Value);
    }

    [Fact]
    public async Task Cout_au_km_non_calculable_avec_un_seul_releve()
    {
        var vehicleId = await CreateVehicleAsync();
        await PostAsync($"/api/vehicles/{vehicleId}/mileage-readings", new { date = "2025-01-10", kilometers = 5000 });
        await PostAsync($"/api/vehicles/{vehicleId}/interventions", new { date = "2025-03-15", work = "Vidange", amount = 250m });

        var response = await _client.GetAsync(SummaryUri($"from=2025-01-01&to=2025-12-31&vehicleId={vehicleId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summary = (await response.Content.ReadFromJsonAsync<SummaryDto>(Json))!;
        Assert.False(summary.CostPerKm.Computable);
        Assert.Null(summary.CostPerKm.Value);
        Assert.Equal(250m, summary.Total);
    }
}
