using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UsMecanic.Api.IntegrationTests;

public class ProblemActionEndpointsTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly HttpClient _anonymous;

    public ProblemActionEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        _anonymous = factory.CreateClient();
    }

    private sealed record IdDto(Guid Id);

    private sealed record ActionDto(
        Guid Id,
        Guid ProblemId,
        Guid ActorId,
        Guid TypeId,
        Guid ConstraintId,
        DateOnly? DueDate,
        string? Comment,
        Guid? InterventionId,
        bool IsClosed,
        DateTimeOffset? ClosedAt);

    private sealed record ProblemDto(Guid Id, Guid VehicleId, string Status, IReadOnlyList<ActionDto> Actions);

    private sealed record Refs(Guid ActorId, Guid TypeId, Guid ConstraintId);

    private static Uri ActionsUri(Guid problemId) => new($"/api/problems/{problemId}/actions", UriKind.Relative);

    private static Uri ActionUri(Guid problemId, Guid actionId) => new($"/api/problems/{problemId}/actions/{actionId}", UriKind.Relative);

    private static Uri CloseUri(Guid problemId, Guid actionId) => new($"/api/problems/{problemId}/actions/{actionId}/close", UriKind.Relative);

    private static Uri ProblemUri(Guid problemId) => new($"/api/problems/{problemId}", UriKind.Relative);

    private static DateOnly Offset(int days) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(days);

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

    private async Task<Refs> CreateRefsAsync() => new(
        await CreateReferenceAsync("/api/problem-actors"),
        await CreateReferenceAsync("/api/problem-action-types"),
        await CreateReferenceAsync("/api/problem-constraints"));

    private async Task<(Guid Id, string Plate)> CreateVehicleAsync()
    {
        var plate = NewPlate();
        var vehicle = await _client.PostAsJsonAsync(new Uri("/api/vehicles", UriKind.Relative), new { licensePlate = plate, details = new { brand = "Renault" } });
        Assert.Equal(HttpStatusCode.Created, vehicle.StatusCode);
        return ((await vehicle.Content.ReadFromJsonAsync<IdDto>(Json))!.Id, plate);
    }

    private async Task<Guid> ReportProblemAsync(string plate, bool analyze)
    {
        var report = await _client.PostAsJsonAsync(
            new Uri("/api/problems", UriKind.Relative),
            new { licensePlate = plate, date = Offset(-1), description = "Voyant moteur", author = "D. Martin" });
        Assert.Equal(HttpStatusCode.Created, report.StatusCode);
        var problemId = (await report.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
        if (analyze)
        {
            var natureId = await CreateReferenceAsync("/api/problem-natures");
            var elementId = await CreateReferenceAsync("/api/problem-elements");
            var analysis = await _client.PutAsJsonAsync(
                new Uri($"/api/problems/{problemId}/analysis", UriKind.Relative),
                new { natureId, elementId, analysis = "Injecteur défaillant", immobilized = false });
            Assert.Equal(HttpStatusCode.OK, analysis.StatusCode);
        }

        return problemId;
    }

    private async Task<(Guid VehicleId, Guid ProblemId)> AnalyzedProblemAsync()
    {
        var (vehicleId, plate) = await CreateVehicleAsync();
        return (vehicleId, await ReportProblemAsync(plate, analyze: true));
    }

    private async Task<Guid> CreateInterventionAsync(Guid vehicleId)
    {
        var response = await _client.PostAsJsonAsync(
            new Uri($"/api/vehicles/{vehicleId}/interventions", UriKind.Relative),
            new { date = Offset(-1), work = "Remplacement injecteur" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<IdDto>(Json))!.Id;
    }

    private Task<HttpResponseMessage> AddActionAsync(Guid problemId, Refs refs, DateOnly? dueDate = null, string? comment = "Prendre rendez-vous", Guid? interventionId = null) =>
        _client.PostAsJsonAsync(
            ActionsUri(problemId),
            new { actorId = refs.ActorId, typeId = refs.TypeId, constraintId = refs.ConstraintId, dueDate, comment, interventionId });

    private async Task<ProblemDto> GetProblemAsync(Guid problemId) =>
        (await _client.GetFromJsonAsync<ProblemDto>(ProblemUri(problemId), Json))!;

    [Fact]
    public async Task Les_actions_exigent_une_authentification()
    {
        var add = await _anonymous.PostAsJsonAsync(ActionsUri(Guid.NewGuid()), new { actorId = Guid.NewGuid(), typeId = Guid.NewGuid(), constraintId = Guid.NewGuid() });
        var update = await _anonymous.PutAsJsonAsync(ActionUri(Guid.NewGuid(), Guid.NewGuid()), new { comment = "x" });
        var close = await _anonymous.PostAsync(CloseUri(Guid.NewGuid(), Guid.NewGuid()), null);

        Assert.Equal(HttpStatusCode.Unauthorized, add.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, update.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, close.StatusCode);
    }

    [Fact]
    public async Task Ajouter_une_action_renvoie_201_et_apparait_dans_le_detail_du_probleme()
    {
        var (_, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var due = Offset(15);

        var response = await AddActionAsync(problemId, refs, due, "  Prendre rendez-vous ");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var action = (await response.Content.ReadFromJsonAsync<ActionDto>(Json))!;
        Assert.Equal(problemId, action.ProblemId);
        Assert.Equal(refs.ActorId, action.ActorId);
        Assert.Equal(refs.TypeId, action.TypeId);
        Assert.Equal(refs.ConstraintId, action.ConstraintId);
        Assert.Equal(due, action.DueDate);
        Assert.Equal("Prendre rendez-vous", action.Comment);
        Assert.False(action.IsClosed);

        var detail = await GetProblemAsync(problemId);
        Assert.Equal("Analyzed", detail.Status);
        var stored = Assert.Single(detail.Actions);
        Assert.Equal(action.Id, stored.Id);
        Assert.Equal(due, stored.DueDate);
        Assert.Equal("Prendre rendez-vous", stored.Comment);
    }

    [Fact]
    public async Task Une_analyse_accepte_plusieurs_actions_dont_les_champs_facultatifs_sont_absents()
    {
        var (_, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();

        var first = await AddActionAsync(problemId, refs, comment: null);
        var second = await AddActionAsync(problemId, refs);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var detail = await GetProblemAsync(problemId);
        Assert.Equal(2, detail.Actions.Count);
        Assert.Contains(detail.Actions, a => a.DueDate is null && a.Comment is null && a.InterventionId is null);
    }

    [Fact]
    public async Task Un_probleme_sans_action_expose_une_liste_vide()
    {
        var (_, problemId) = await AnalyzedProblemAsync();

        Assert.Empty((await GetProblemAsync(problemId)).Actions);
    }

    [Fact]
    public async Task Une_action_sur_un_probleme_non_analyse_est_refusee_en_conflit()
    {
        var (_, plate) = await CreateVehicleAsync();
        var problemId = await ReportProblemAsync(plate, analyze: false);
        var refs = await CreateRefsAsync();

        var response = await AddActionAsync(problemId, refs);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("problem.not_analyzed", await ErrorCodeAsync(response));
        Assert.Empty((await GetProblemAsync(problemId)).Actions);
    }

    [Fact]
    public async Task Un_probleme_ou_une_action_inconnu_renvoie_404()
    {
        var (_, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();

        var addUnknown = await AddActionAsync(Guid.NewGuid(), refs);
        var updateUnknown = await _client.PutAsJsonAsync(ActionUri(problemId, Guid.NewGuid()), new { comment = "x" });
        var closeUnknown = await _client.PostAsync(CloseUri(problemId, Guid.NewGuid()), null);
        var closeNoProblem = await _client.PostAsync(CloseUri(Guid.NewGuid(), Guid.NewGuid()), null);

        Assert.Equal(HttpStatusCode.NotFound, addUnknown.StatusCode);
        Assert.Equal("problem.not_found", await ErrorCodeAsync(addUnknown));
        Assert.Equal(HttpStatusCode.NotFound, updateUnknown.StatusCode);
        Assert.Equal("problem_action.not_found", await ErrorCodeAsync(updateUnknown));
        Assert.Equal(HttpStatusCode.NotFound, closeUnknown.StatusCode);
        Assert.Equal("problem_action.not_found", await ErrorCodeAsync(closeUnknown));
        Assert.Equal(HttpStatusCode.NotFound, closeNoProblem.StatusCode);
        Assert.Equal("problem.not_found", await ErrorCodeAsync(closeNoProblem));
    }

    [Fact]
    public async Task Un_referentiel_inconnu_ou_inactif_est_refuse()
    {
        var (_, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var inactiveActor = await CreateReferenceAsync("/api/problem-actors");
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsync(new Uri($"/api/problem-actors/{inactiveActor}/deactivate", UriKind.Relative), null)).StatusCode);

        var unknownActor = await AddActionAsync(problemId, refs with { ActorId = Guid.NewGuid() });
        var unknownType = await AddActionAsync(problemId, refs with { TypeId = Guid.NewGuid() });
        var unknownConstraint = await AddActionAsync(problemId, refs with { ConstraintId = Guid.NewGuid() });
        var inactive = await AddActionAsync(problemId, refs with { ActorId = inactiveActor });

        Assert.Equal(HttpStatusCode.BadRequest, unknownActor.StatusCode);
        Assert.Equal("problem_action.actor_unknown", await ErrorCodeAsync(unknownActor));
        Assert.Equal(HttpStatusCode.BadRequest, unknownType.StatusCode);
        Assert.Equal("problem_action.type_unknown", await ErrorCodeAsync(unknownType));
        Assert.Equal(HttpStatusCode.BadRequest, unknownConstraint.StatusCode);
        Assert.Equal("problem_action.constraint_unknown", await ErrorCodeAsync(unknownConstraint));
        Assert.Equal(HttpStatusCode.BadRequest, inactive.StatusCode);
        Assert.Equal("problem_actor.inactive", await ErrorCodeAsync(inactive));
        Assert.Empty((await GetProblemAsync(problemId)).Actions);
    }

    [Fact]
    public async Task Une_intervention_du_meme_vehicule_est_liee_et_celle_d_un_autre_vehicule_est_refusee()
    {
        var (vehicleId, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var interventionId = await CreateInterventionAsync(vehicleId);
        var foreignIntervention = await CreateInterventionAsync((await CreateVehicleAsync()).Id);

        var linked = await AddActionAsync(problemId, refs, interventionId: interventionId);
        var foreign = await AddActionAsync(problemId, refs, interventionId: foreignIntervention);
        var unknown = await AddActionAsync(problemId, refs, interventionId: Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Created, linked.StatusCode);
        Assert.Equal(interventionId, (await linked.Content.ReadFromJsonAsync<ActionDto>(Json))!.InterventionId);
        Assert.Equal(HttpStatusCode.BadRequest, foreign.StatusCode);
        Assert.Equal("problem_action.intervention_invalid", await ErrorCodeAsync(foreign));
        Assert.Equal(HttpStatusCode.BadRequest, unknown.StatusCode);
        Assert.Equal("problem_action.intervention_invalid", await ErrorCodeAsync(unknown));
        Assert.Single((await GetProblemAsync(problemId)).Actions);
    }

    [Fact]
    public async Task Modifier_une_action_met_a_jour_echeance_commentaire_et_intervention()
    {
        var (vehicleId, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var action = (await (await AddActionAsync(problemId, refs, Offset(5), "Avant")).Content.ReadFromJsonAsync<ActionDto>(Json))!;
        var interventionId = await CreateInterventionAsync(vehicleId);
        var due = Offset(20);

        var response = await _client.PutAsJsonAsync(ActionUri(problemId, action.Id), new { dueDate = due, comment = "Après", interventionId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<ActionDto>(Json))!;
        Assert.Equal(due, updated.DueDate);
        Assert.Equal("Après", updated.Comment);
        Assert.Equal(interventionId, updated.InterventionId);
        var stored = Assert.Single((await GetProblemAsync(problemId)).Actions);
        Assert.Equal("Après", stored.Comment);
        Assert.Equal(interventionId, stored.InterventionId);
        Assert.Equal(refs.ActorId, stored.ActorId);
    }

    [Fact]
    public async Task Modifier_avec_une_intervention_d_un_autre_vehicule_est_refuse()
    {
        var (_, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var action = (await (await AddActionAsync(problemId, refs)).Content.ReadFromJsonAsync<ActionDto>(Json))!;
        var foreignIntervention = await CreateInterventionAsync((await CreateVehicleAsync()).Id);

        var response = await _client.PutAsJsonAsync(ActionUri(problemId, action.Id), new { comment = "Après", interventionId = foreignIntervention });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("problem_action.intervention_invalid", await ErrorCodeAsync(response));
    }

    [Fact]
    public async Task Cloturer_une_action_renseigne_la_date_et_une_seconde_cloture_ou_modification_est_refusee()
    {
        var (_, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var action = (await (await AddActionAsync(problemId, refs)).Content.ReadFromJsonAsync<ActionDto>(Json))!;

        var close = await _client.PostAsync(CloseUri(problemId, action.Id), null);

        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
        var closed = (await close.Content.ReadFromJsonAsync<ActionDto>(Json))!;
        Assert.True(closed.IsClosed);
        Assert.NotNull(closed.ClosedAt);
        var stored = Assert.Single((await GetProblemAsync(problemId)).Actions);
        Assert.True(stored.IsClosed);
        // PostgreSQL stocke les horodatages à la microseconde : la réponse de la clôture est plus précise.
        Assert.Equal(closed.ClosedAt!.Value, stored.ClosedAt!.Value, TimeSpan.FromMilliseconds(1));

        var again = await _client.PostAsync(CloseUri(problemId, action.Id), null);
        var update = await _client.PutAsJsonAsync(ActionUri(problemId, action.Id), new { comment = "Trop tard" });

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("problem_action.already_closed", await ErrorCodeAsync(again));
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        Assert.Equal("problem_action.already_closed", await ErrorCodeAsync(update));
        Assert.Equal("Resolved", (await GetProblemAsync(problemId)).Status);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_refuse_l_ajout_la_modification_et_la_cloture()
    {
        var (vehicleId, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var action = (await (await AddActionAsync(problemId, refs)).Content.ReadFromJsonAsync<ActionDto>(Json))!;
        var retired = await _client.PutAsJsonAsync(new Uri($"/api/vehicles/{vehicleId}/status", UriKind.Relative), new { status = "Retired" });
        Assert.Equal(HttpStatusCode.OK, retired.StatusCode);

        var add = await AddActionAsync(problemId, refs);
        var update = await _client.PutAsJsonAsync(ActionUri(problemId, action.Id), new { comment = "Après" });
        var close = await _client.PostAsync(CloseUri(problemId, action.Id), null);

        foreach (var response in new[] { add, update, close })
        {
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("vehicle.retired", await ErrorCodeAsync(response));
        }

        var stored = Assert.Single((await GetProblemAsync(problemId)).Actions);
        Assert.False(stored.IsClosed);
    }

    [Fact]
    public async Task Supprimer_l_intervention_liee_retire_le_lien_et_conserve_l_action()
    {
        var (vehicleId, problemId) = await AnalyzedProblemAsync();
        var refs = await CreateRefsAsync();
        var interventionId = await CreateInterventionAsync(vehicleId);
        var action = (await (await AddActionAsync(problemId, refs, comment: "Garder", interventionId: interventionId)).Content.ReadFromJsonAsync<ActionDto>(Json))!;
        Assert.Equal(interventionId, (await GetProblemAsync(problemId)).Actions[0].InterventionId);

        var delete = await _client.DeleteAsync(new Uri($"/api/vehicles/{vehicleId}/interventions/{interventionId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var stored = Assert.Single((await GetProblemAsync(problemId)).Actions);
        Assert.Equal(action.Id, stored.Id);
        Assert.Equal("Garder", stored.Comment);
        Assert.Null(stored.InterventionId);
    }
}
