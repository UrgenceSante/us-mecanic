using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class ProblemAutoStatusEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;

    public ProblemAutoStatusEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record VehicleDto(Guid Id, string Status);

    private sealed record ActionDto(Guid Id, bool IsClosed, DateTimeOffset? ClosedAt);

    private sealed record ProblemDto(Guid Id, string Status, IReadOnlyList<ActionDto> Actions);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private async Task<Guid> CreateReferenceAsync(string route)
    {
        var response = await _client.PostAsJsonAsync(new Uri(route, UriKind.Relative), new { name = $"Valeur {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<Guid> CreateVehicleAsync(string plate)
    {
        var response = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = plate, details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<Guid> AnalyzedProblemAsync(string plate, bool immobilized)
    {
        var report = await _client.PostAsJsonAsync(
            new Uri("/api/problems", UriKind.Relative),
            new { licensePlate = plate, date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), description = "Voyant moteur", author = "D. Martin" });
        Assert.Equal(HttpStatusCode.Created, report.StatusCode);
        var problemId = (await report.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
        var analysis = await _client.PutAsJsonAsync(
            new Uri($"/api/problems/{problemId}/analysis", UriKind.Relative),
            new
            {
                natureId = await CreateReferenceAsync("/api/problem-natures"),
                elementId = await CreateReferenceAsync("/api/problem-elements"),
                analysis = "Injecteur défaillant",
                immobilized,
            });
        Assert.Equal(HttpStatusCode.OK, analysis.StatusCode);
        return problemId;
    }

    private async Task<Guid> AddActionAsync(Guid problemId)
    {
        var response = await _client.PostAsJsonAsync(
            new Uri($"/api/problems/{problemId}/actions", UriKind.Relative),
            new
            {
                actorId = await CreateReferenceAsync("/api/problem-actors"),
                typeId = await CreateReferenceAsync("/api/problem-action-types"),
                constraintId = await CreateReferenceAsync("/api/problem-constraints"),
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ActionDto>(Json))!.Id;
    }

    private async Task CloseActionAsync(Guid problemId, Guid actionId)
    {
        var response = await _client.PostAsync(new Uri($"/api/problems/{problemId}/actions/{actionId}/close", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<ProblemDto> GetProblemAsync(Guid problemId) =>
        (await _client.GetFromJsonAsync<ProblemDto>(new Uri($"/api/problems/{problemId}", UriKind.Relative), Json))!;

    private async Task<string> VehicleStatusAsync(Guid vehicleId) =>
        (await _client.GetFromJsonAsync<VehicleDto>(new Uri($"/api/vehicles/{vehicleId}", UriKind.Relative), Json))!.Status;

    [Fact]
    public async Task Cloturer_la_derniere_action_resout_le_probleme_et_un_probleme_sans_action_reste_analyse()
    {
        var plate = NewPlate();
        await CreateVehicleAsync(plate);
        var problemId = await AnalyzedProblemAsync(plate, immobilized: false);
        Assert.Equal("Analyzed", (await GetProblemAsync(problemId)).Status);

        var first = await AddActionAsync(problemId);
        var second = await AddActionAsync(problemId);
        await CloseActionAsync(problemId, first);
        Assert.Equal("Analyzed", (await GetProblemAsync(problemId)).Status);

        await CloseActionAsync(problemId, second);

        var detail = await GetProblemAsync(problemId);
        Assert.Equal("Resolved", detail.Status);
        Assert.All(detail.Actions, a => Assert.True(a.IsClosed));
    }

    [Fact]
    public async Task Les_actions_sont_listees_de_la_plus_ancienne_a_la_plus_recente()
    {
        var plate = NewPlate();
        await CreateVehicleAsync(plate);
        var problemId = await AnalyzedProblemAsync(plate, immobilized: false);
        var ids = new List<Guid>();
        for (var i = 0; i < 3; i++)
        {
            ids.Add(await AddActionAsync(problemId));
        }

        var detail = await GetProblemAsync(problemId);

        Assert.Equal(ids, detail.Actions.Select(a => a.Id).ToList());
    }

    [Fact]
    public async Task Ajouter_une_action_a_un_probleme_resolu_le_rouvre()
    {
        var plate = NewPlate();
        await CreateVehicleAsync(plate);
        var problemId = await AnalyzedProblemAsync(plate, immobilized: false);
        await CloseActionAsync(problemId, await AddActionAsync(problemId));
        Assert.Equal("Resolved", (await GetProblemAsync(problemId)).Status);

        await AddActionAsync(problemId);

        Assert.Equal("Analyzed", (await GetProblemAsync(problemId)).Status);
    }

    [Fact]
    public async Task Le_vehicule_repasse_en_service_seulement_sans_autre_probleme_immobilisant_ouvert_puis_se_re_immobilise_a_la_reouverture()
    {
        var plate = NewPlate();
        var vehicleId = await CreateVehicleAsync(plate);
        var first = await AnalyzedProblemAsync(plate, immobilized: true);
        var second = await AnalyzedProblemAsync(plate, immobilized: true);
        Assert.Equal("Immobilized", await VehicleStatusAsync(vehicleId));
        var firstAction = await AddActionAsync(first);
        var secondAction = await AddActionAsync(second);

        await CloseActionAsync(first, firstAction);
        Assert.Equal("Resolved", (await GetProblemAsync(first)).Status);
        Assert.Equal("Immobilized", await VehicleStatusAsync(vehicleId));

        await CloseActionAsync(second, secondAction);
        Assert.Equal("Active", await VehicleStatusAsync(vehicleId));

        await AddActionAsync(first);
        Assert.Equal("Analyzed", (await GetProblemAsync(first)).Status);
        Assert.Equal("Immobilized", await VehicleStatusAsync(vehicleId));
    }

    [Fact]
    public async Task Resoudre_un_probleme_non_immobilisant_ne_remet_pas_en_service_un_vehicule_immobilise_par_un_autre()
    {
        var plate = NewPlate();
        var vehicleId = await CreateVehicleAsync(plate);
        await AnalyzedProblemAsync(plate, immobilized: true);
        var light = await AnalyzedProblemAsync(plate, immobilized: false);

        await CloseActionAsync(light, await AddActionAsync(light));

        Assert.Equal("Resolved", (await GetProblemAsync(light)).Status);
        Assert.Equal("Immobilized", await VehicleStatusAsync(vehicleId));
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_garde_son_statut()
    {
        var plate = NewPlate();
        var vehicleId = await CreateVehicleAsync(plate);
        var problemId = await AnalyzedProblemAsync(plate, immobilized: true);
        var actionId = await AddActionAsync(problemId);
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        var close = await _client.PostAsync(new Uri($"/api/problems/{problemId}/actions/{actionId}/close", UriKind.Relative), null);

        Assert.Equal(HttpStatusCode.Conflict, close.StatusCode);
        Assert.Equal("Retired", await VehicleStatusAsync(vehicleId));
        Assert.Equal("Analyzed", (await GetProblemAsync(problemId)).Status);
    }
}
