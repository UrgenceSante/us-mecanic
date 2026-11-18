using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class VehicleAlertsEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public VehicleAlertsEndpointTests(ApiFactory factory) => _client = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task La_liste_expose_les_alertes_en_camel_case_et_la_fiche_non()
    {
        var plate = "ZA-" + Random.Shared.Next(100, 999) + "-ZB";
        var created = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = plate, details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var list = await _client.GetFromJsonAsync<JsonElement>(new Uri("/api/vehicles", UriKind.Relative));
        var item = list.EnumerateArray().Single(v => v.GetProperty("id").GetGuid() == id);
        var alerts = item.GetProperty("alerts");
        Assert.Equal(0, alerts.GetProperty("overdueDeadlines").GetInt32());
        Assert.False(alerts.GetProperty("immobilizingProblem").GetBoolean());

        var detail = await _client.GetFromJsonAsync<JsonElement>(new Uri($"/api/vehicles/{id}", UriKind.Relative));
        Assert.False(detail.TryGetProperty("alerts", out var value) && value.ValueKind != JsonValueKind.Null);
    }
}
