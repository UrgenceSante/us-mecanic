using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class ProblemFleetListEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly string[] ExpectedLevels = ["Immobilizing", "Immobilizing", "Unanalyzed", "OverdueAction", "Other"];

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public ProblemFleetListEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record ProblemLine(Guid Id, Guid VehicleId, string Status);

    private sealed record VehicleLine(Guid Id, string LicensePlate);

    private sealed record NatureLine(Guid Id, string Name);

    private sealed record Line(ProblemLine Problem, VehicleLine Vehicle, NatureLine? Nature, string BlockingLevel);

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

    private Task<Guid> CreateReferenceAsync(string route) => PostIdAsync(route, new { name = $"Valeur {Guid.NewGuid():N}" });

    private async Task<(Guid Id, string Plate)> CreateVehicleAsync()
    {
        var plate = NewPlate();
        return (await PostIdAsync("/api/vehicles", new { licensePlate = plate, details = new { brand = "Renault" } }), plate);
    }

    private Task<Guid> ReportAsync(string plate, int daysAgo) =>
        PostIdAsync("/api/problems", new { licensePlate = plate, date = Offset(-daysAgo), description = "Voyant moteur", author = "D. Martin" });

    private async Task<Guid> AnalyzeAsync(Guid problemId, bool immobilized, Guid? natureId = null)
    {
        var nature = natureId ?? await CreateReferenceAsync("/api/problem-natures");
        var response = await _client.PutAsJsonAsync(
            new Uri($"/api/problems/{problemId}/analysis", UriKind.Relative),
            new { natureId = nature, elementId = await CreateReferenceAsync("/api/problem-elements"), analysis = "Injecteur HS", immobilized });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return nature;
    }

    private async Task<Guid> AddActionAsync(Guid problemId, DateOnly? dueDate)
    {
        return await PostIdAsync(
            $"/api/problems/{problemId}/actions",
            new
            {
                actorId = await CreateReferenceAsync("/api/problem-actors"),
                typeId = await CreateReferenceAsync("/api/problem-action-types"),
                constraintId = await CreateReferenceAsync("/api/problem-constraints"),
                dueDate,
            });
    }

    private async Task CloseActionAsync(Guid problemId, Guid actionId)
    {
        var response = await _client.PostAsync(new Uri($"/api/problems/{problemId}/actions/{actionId}/close", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<List<Line>> ListAsync(string query)
    {
        var response = await _client.GetAsync(new Uri($"/api/problems?{query}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<List<Line>>(Json))!;
    }

    [Fact]
    public async Task La_liste_suit_les_quatre_niveaux_puis_l_anciennete()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();
        var other = await ReportAsync(plate, daysAgo: 30);
        await AnalyzeAsync(other, immobilized: false);
        var overdue = await ReportAsync(plate, daysAgo: 12);
        await AnalyzeAsync(overdue, immobilized: false);
        await AddActionAsync(overdue, Offset(-5));
        var unanalyzed = await ReportAsync(plate, daysAgo: 2);
        var immobilizingRecent = await ReportAsync(plate, daysAgo: 3);
        await AnalyzeAsync(immobilizingRecent, immobilized: true);
        var immobilizingOld = await ReportAsync(plate, daysAgo: 9);
        await AnalyzeAsync(immobilizingOld, immobilized: true);

        var list = await ListAsync($"vehicleId={vehicleId}");

        Guid[] expectedIds = [immobilizingOld, immobilizingRecent, unanalyzed, overdue, other];
        Assert.Equal(expectedIds, list.Select(l => l.Problem.Id).ToArray());
        Assert.Equal(ExpectedLevels, list.Select(l => l.BlockingLevel).ToArray());
        Assert.All(list, l => Assert.Equal(new VehicleLine(vehicleId, plate), l.Vehicle));
        Assert.Null(list[2].Nature);
        Assert.NotNull(list[0].Nature);
    }

    [Fact]
    public async Task Une_action_close_ou_a_echeance_future_ne_classe_pas_en_retard()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();
        var closed = await ReportAsync(plate, daysAgo: 8);
        await AnalyzeAsync(closed, immobilized: false);
        await CloseActionAsync(closed, await AddActionAsync(closed, Offset(-4)));
        await AddActionAsync(closed, null);
        var future = await ReportAsync(plate, daysAgo: 7);
        await AnalyzeAsync(future, immobilized: false);
        await AddActionAsync(future, Offset(4));

        var list = await ListAsync($"vehicleId={vehicleId}");

        Assert.Equal(2, list.Count);
        Assert.All(list, l => Assert.Equal("Other", l.BlockingLevel));
    }

    [Fact]
    public async Task Les_resolus_sont_exclus_par_defaut_et_renvoyes_avec_le_statut_Resolved()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();
        var resolved = await ReportAsync(plate, daysAgo: 5);
        await AnalyzeAsync(resolved, immobilized: false);
        await CloseActionAsync(resolved, await AddActionAsync(resolved, Offset(-1)));
        var open = await ReportAsync(plate, daysAgo: 4);

        var byDefault = await ListAsync($"vehicleId={vehicleId}");
        var onlyResolved = await ListAsync($"vehicleId={vehicleId}&status=Resolved");
        var onlyReported = await ListAsync($"vehicleId={vehicleId}&status=Reported");

        Assert.Equal(open, Assert.Single(byDefault).Problem.Id);
        Assert.Equal(resolved, Assert.Single(onlyResolved).Problem.Id);
        Assert.Equal("Other", onlyResolved[0].BlockingLevel);
        Assert.Equal(open, Assert.Single(onlyReported).Problem.Id);
    }

    [Fact]
    public async Task Les_filtres_vehicule_et_nature_se_combinent()
    {
        var (firstId, firstPlate) = await CreateVehicleAsync();
        var (secondId, secondPlate) = await CreateVehicleAsync();
        var natureId = await CreateReferenceAsync("/api/problem-natures");
        var wanted = await ReportAsync(firstPlate, daysAgo: 3);
        await AnalyzeAsync(wanted, immobilized: false, natureId);
        var otherNature = await ReportAsync(firstPlate, daysAgo: 2);
        await AnalyzeAsync(otherNature, immobilized: false);
        var otherVehicle = await ReportAsync(secondPlate, daysAgo: 4);
        await AnalyzeAsync(otherVehicle, immobilized: false, natureId);

        var byNature = await ListAsync($"natureId={natureId}");
        var combined = await ListAsync($"vehicleId={firstId}&natureId={natureId}&status=Analyzed");
        var unknownNature = await ListAsync($"vehicleId={secondId}&natureId={Guid.NewGuid()}");

        Guid[] expectedByNature = [otherVehicle, wanted];
        Assert.Equal(expectedByNature, byNature.Select(l => l.Problem.Id).ToArray());
        Assert.Equal(wanted, Assert.Single(combined).Problem.Id);
        Assert.Equal(natureId, combined[0].Nature!.Id);
        Assert.Empty(unknownNature);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_voit_ses_problemes_masques()
    {
        var (retiredId, retiredPlate) = await CreateVehicleAsync();
        var hidden = await ReportAsync(retiredPlate, daysAgo: 3);
        var (keptId, keptPlate) = await CreateVehicleAsync();
        var shown = await ReportAsync(keptPlate, daysAgo: 3);
        var retire = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{retiredId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retire.StatusCode);

        var all = await ListAsync(string.Empty);

        Assert.Empty(await ListAsync($"vehicleId={retiredId}"));
        Assert.DoesNotContain(all, l => l.Problem.Id == hidden);
        Assert.Contains(all, l => l.Problem.Id == shown);
        Assert.Equal(shown, Assert.Single(await ListAsync($"vehicleId={keptId}")).Problem.Id);
    }

    [Fact]
    public async Task Un_statut_inconnu_est_refuse_en_400()
    {
        var response = await _client.GetAsync(new Uri("/api/problems?status=Nimporte", UriKind.Relative));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task La_liste_exige_une_authentification()
    {
        var response = await _anonymous.GetAsync(new Uri("/api/problems", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
