using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class ProblemAnalysisEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public ProblemAnalysisEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record VehicleDto(Guid Id, string Status);

    private sealed record AnalysisDto(Guid NatureId, Guid ElementId, string Text, bool Immobilized, DateTimeOffset AnalyzedAt);

    private sealed record ProblemDto(Guid Id, Guid VehicleId, string Status, AnalysisDto? Analysis);

    private static Uri AnalysisUri(Guid problemId) => new($"/api/problems/{problemId}/analysis", UriKind.Relative);

    private static Uri ProblemUri(Guid problemId) => new($"/api/problems/{problemId}", UriKind.Relative);

    private static string NewPlate()
    {
        var n = Random.Shared.Next(0, 26 * 26 * 1000 * 26 * 26);
        static char L(int i) => (char)('A' + (i % 26));
        return $"{L(n)}{L(n / 26)}-{n / 676 % 1000:000}-{L(n / 676000)}{L(n / 17576000)}";
    }

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return problem.RootElement.GetProperty("code").GetString();
    }

    private async Task<Guid> CreateReferenceAsync(string route)
    {
        var response = await _client.PostAsJsonAsync(new Uri(route, UriKind.Relative), new { name = $"Valeur {Guid.NewGuid():N}" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private async Task<(Guid VehicleId, Guid ProblemId)> ReportProblemAsync()
    {
        var plate = NewPlate();
        var vehicle = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = plate, details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, vehicle.StatusCode);
        var vehicleId = (await vehicle.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
        var problem = await _client.PostAsJsonAsync(
            new Uri("/api/problems", UriKind.Relative),
            new { licensePlate = plate, date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), description = "Voyant moteur", author = "D. Martin" });
        Assert.Equal(HttpStatusCode.Created, problem.StatusCode);
        return (vehicleId, (await problem.Content.ReadFromJsonAsync<IdDto>(Json))!.Id);
    }

    private async Task<string> VehicleStatusAsync(Guid vehicleId) =>
        (await _client.GetFromJsonAsync<VehicleDto>(new Uri($"/api/vehicles/{vehicleId}", UriKind.Relative), Json))!.Status;

    private Task<HttpResponseMessage> AnalyzeAsync(Guid problemId, Guid natureId, Guid elementId, bool immobilized = false, string? analysis = "Injecteur défaillant") =>
        _client.PutAsJsonAsync(AnalysisUri(problemId), new { natureId, elementId, analysis, immobilized });

    [Fact]
    public async Task L_analyse_exige_une_authentification()
    {
        var write = await _anonymous.PutAsJsonAsync(AnalysisUri(Guid.NewGuid()), new { natureId = Guid.NewGuid(), elementId = Guid.NewGuid(), analysis = "x", immobilized = false });
        var read = await _anonymous.GetAsync(ProblemUri(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, read.StatusCode);
    }

    [Fact]
    public async Task Une_analyse_sans_immobilisation_passe_le_probleme_a_analyse_et_laisse_le_vehicule_en_service()
    {
        var (vehicleId, problemId) = await ReportProblemAsync();
        var natureId = await CreateReferenceAsync("/api/problem-natures");
        var elementId = await CreateReferenceAsync("/api/problem-elements");

        var response = await AnalyzeAsync(problemId, natureId, elementId, analysis: "  Injecteur défaillant  ");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDto>(Json))!;
        Assert.Equal("Analyzed", problem.Status);
        Assert.Equal(natureId, problem.Analysis!.NatureId);
        Assert.Equal(elementId, problem.Analysis.ElementId);
        Assert.Equal("Injecteur défaillant", problem.Analysis.Text);
        Assert.False(problem.Analysis.Immobilized);
        Assert.Equal("Active", await VehicleStatusAsync(vehicleId));

        var detail = (await _client.GetFromJsonAsync<ProblemDto>(ProblemUri(problemId), Json))!;
        Assert.Equal("Analyzed", detail.Status);
        // PostgreSQL stocke les horodatages à la microseconde : la réponse du PUT est plus précise.
        Assert.Equal(problem.Analysis.AnalyzedAt, detail.Analysis!.AnalyzedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Une_analyse_avec_immobilisation_immobilise_le_vehicule()
    {
        var (vehicleId, problemId) = await ReportProblemAsync();
        var natureId = await CreateReferenceAsync("/api/problem-natures");
        var elementId = await CreateReferenceAsync("/api/problem-elements");

        var response = await AnalyzeAsync(problemId, natureId, elementId, immobilized: true);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await response.Content.ReadFromJsonAsync<ProblemDto>(Json))!.Analysis!.Immobilized);
        Assert.Equal("Immobilized", await VehicleStatusAsync(vehicleId));
    }

    [Fact]
    public async Task Le_detail_d_un_probleme_signale_n_a_pas_d_analyse_et_un_probleme_inconnu_renvoie_404()
    {
        var (_, problemId) = await ReportProblemAsync();

        var detail = await _client.GetAsync(ProblemUri(problemId));
        var unknown = await _client.GetAsync(ProblemUri(Guid.NewGuid()));
        var unknownAnalysis = await AnalyzeAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var problem = (await detail.Content.ReadFromJsonAsync<ProblemDto>(Json))!;
        Assert.Equal("Reported", problem.Status);
        Assert.Null(problem.Analysis);
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("problem.not_found", await ErrorCodeAsync(unknown));
        Assert.Equal(HttpStatusCode.NotFound, unknownAnalysis.StatusCode);
        Assert.Equal("problem.not_found", await ErrorCodeAsync(unknownAnalysis));
    }

    [Fact]
    public async Task Une_seconde_analyse_est_refusee_en_conflit_et_laisse_la_premiere_intacte()
    {
        var (vehicleId, problemId) = await ReportProblemAsync();
        var natureId = await CreateReferenceAsync("/api/problem-natures");
        var elementId = await CreateReferenceAsync("/api/problem-elements");
        await AnalyzeAsync(problemId, natureId, elementId, analysis: "Première");

        var second = await AnalyzeAsync(problemId, natureId, elementId, immobilized: true, analysis: "Seconde");

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("problem.already_analyzed", await ErrorCodeAsync(second));
        var detail = (await _client.GetFromJsonAsync<ProblemDto>(ProblemUri(problemId), Json))!;
        Assert.Equal("Première", detail.Analysis!.Text);
        Assert.Equal("Active", await VehicleStatusAsync(vehicleId));
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_refuse_l_analyse()
    {
        var (vehicleId, problemId) = await ReportProblemAsync();
        var natureId = await CreateReferenceAsync("/api/problem-natures");
        var elementId = await CreateReferenceAsync("/api/problem-elements");
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        var response = await AnalyzeAsync(problemId, natureId, elementId, immobilized: true);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("vehicle.retired", await ErrorCodeAsync(response));
        Assert.Equal("Retired", await VehicleStatusAsync(vehicleId));
        Assert.Equal("Reported", (await _client.GetFromJsonAsync<ProblemDto>(ProblemUri(problemId), Json))!.Status);
    }

    [Fact]
    public async Task Une_nature_ou_un_element_inconnu_ou_inactif_est_refuse()
    {
        var (_, problemId) = await ReportProblemAsync();
        var natureId = await CreateReferenceAsync("/api/problem-natures");
        var elementId = await CreateReferenceAsync("/api/problem-elements");
        var inactiveNature = await CreateReferenceAsync("/api/problem-natures");
        var inactiveElement = await CreateReferenceAsync("/api/problem-elements");
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsync(new Uri($"/api/problem-natures/{inactiveNature}/deactivate", UriKind.Relative), null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsync(new Uri($"/api/problem-elements/{inactiveElement}/deactivate", UriKind.Relative), null)).StatusCode);

        var unknownNature = await AnalyzeAsync(problemId, Guid.NewGuid(), elementId);
        var unknownElement = await AnalyzeAsync(problemId, natureId, Guid.NewGuid());
        var natureOff = await AnalyzeAsync(problemId, inactiveNature, elementId);
        var elementOff = await AnalyzeAsync(problemId, natureId, inactiveElement);

        Assert.Equal(HttpStatusCode.BadRequest, unknownNature.StatusCode);
        Assert.Equal("problem.nature_unknown", await ErrorCodeAsync(unknownNature));
        Assert.Equal(HttpStatusCode.BadRequest, unknownElement.StatusCode);
        Assert.Equal("problem.element_unknown", await ErrorCodeAsync(unknownElement));
        Assert.Equal(HttpStatusCode.BadRequest, natureOff.StatusCode);
        Assert.Equal("problem_nature.inactive", await ErrorCodeAsync(natureOff));
        Assert.Equal(HttpStatusCode.BadRequest, elementOff.StatusCode);
        Assert.Equal("problem_element.inactive", await ErrorCodeAsync(elementOff));
        Assert.Equal("Reported", (await _client.GetFromJsonAsync<ProblemDto>(ProblemUri(problemId), Json))!.Status);
    }

    [Fact]
    public async Task Un_texte_d_analyse_absent_est_refuse()
    {
        var (vehicleId, problemId) = await ReportProblemAsync();
        var natureId = await CreateReferenceAsync("/api/problem-natures");
        var elementId = await CreateReferenceAsync("/api/problem-elements");

        var response = await AnalyzeAsync(problemId, natureId, elementId, immobilized: true, analysis: " ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("problem.analysis_required", await ErrorCodeAsync(response));
        Assert.Equal("Active", await VehicleStatusAsync(vehicleId));
    }
}
