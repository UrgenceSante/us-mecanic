using UsMecanic.Application.Mileage;
using UsMecanic.Application.Problems;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Problems;

public class ProblemActionUseCasesTests
{
    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryProblemRepository _problems = new();
    private readonly InMemoryInterventionRepository _interventions = new();
    private readonly InMemoryProblemReferenceRepository<ProblemActor> _actors = new();
    private readonly InMemoryProblemReferenceRepository<ProblemActionType> _types = new();
    private readonly InMemoryProblemReferenceRepository<ProblemConstraint> _constraints = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;
    private readonly Problem _problem;
    private readonly ProblemActor _actor;
    private readonly ProblemActionType _type;
    private readonly ProblemConstraint _constraint;

    public ProblemActionUseCasesTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
        _problem = NewProblem(_vehicle, analyzed: true);
        _actor = _actors.Seed("Garage Martin");
        _type = _types.Seed("Réparation");
        _constraint = _constraints.Seed("Pièce à commander");
    }

    private MileageVehicleGuard Guard => new(_vehicles);

    private ProblemActionInterventionCheck Check => new(_interventions);

    private ProblemVehicleStatusSync Sync => new(_problems, _vehicles);

    private AddProblemAction AddUseCase => new(_problems, Guard, _actors, _types, _constraints, Check, Sync, _clock);

    private Problem NewProblem(Vehicle vehicle, bool analyzed, bool immobilized = false)
    {
        var problem = Problem.Create(vehicle.Id, FixedClock.DefaultToday, "Voyant moteur", "D. Martin", FixedClock.DefaultToday, _clock.Now).Value;
        if (analyzed)
        {
            problem.Analyze(Guid.NewGuid(), Guid.NewGuid(), "Injecteur HS", immobilized, _clock.Now);
        }

        _problems.Saved.Add(problem);
        return problem;
    }

    private Intervention SeedIntervention(Vehicle vehicle)
    {
        var intervention = Intervention.Create(vehicle.Id, FixedClock.DefaultToday, null, null, "Vidange", [], FixedClock.DefaultToday, _clock.Now).Value;
        _interventions.Saved.Add(intervention);
        return intervention;
    }

    private Task<Result<ProblemActionDto>> AddAsync(
        Guid? problemId = null, Guid? actorId = null, Guid? typeId = null, Guid? constraintId = null, DateOnly? dueDate = null, string? comment = "RDV pris", Guid? interventionId = null) =>
        AddUseCase.ExecuteAsync(
            problemId ?? _problem.Id,
            new AddProblemActionInput(actorId ?? _actor.Id, typeId ?? _type.Id, constraintId ?? _constraint.Id, dueDate, comment, interventionId),
            CancellationToken.None);

    [Fact]
    public async Task Ajouter_une_action_la_rattache_au_probleme_analyse_en_une_sauvegarde()
    {
        var intervention = SeedIntervention(_vehicle);
        var due = FixedClock.DefaultToday.AddDays(7);
        var saves = _problems.SaveCount;

        var result = await AddAsync(dueDate: due, interventionId: intervention.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(_problem.Id, result.Value.ProblemId);
        Assert.Equal(_actor.Id, result.Value.ActorId);
        Assert.Equal(_type.Id, result.Value.TypeId);
        Assert.Equal(_constraint.Id, result.Value.ConstraintId);
        Assert.Equal(due, result.Value.DueDate);
        Assert.Equal("RDV pris", result.Value.Comment);
        Assert.Equal(intervention.Id, result.Value.InterventionId);
        Assert.False(result.Value.IsClosed);
        Assert.Equal(saves + 1, _problems.SaveCount);
        Assert.Single(_problem.Actions);
        Assert.Equal(ProblemStatus.Analyzed, _problem.Status);
    }

    [Fact]
    public async Task Le_detail_du_probleme_expose_ses_actions()
    {
        await AddAsync();

        var detail = await new GetProblem(_problems).ExecuteAsync(_problem.Id, CancellationToken.None);

        Assert.Single(detail.Value.Actions!);
    }

    [Fact]
    public async Task Un_probleme_inconnu_est_refuse()
    {
        var result = await AddAsync(problemId: Guid.NewGuid());

        Assert.Equal(ProblemErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Une_action_sur_un_probleme_non_analyse_est_refusee()
    {
        var reported = NewProblem(_vehicle, analyzed: false);

        var result = await AddAsync(problemId: reported.Id);

        Assert.Equal(ProblemErrors.NotAnalyzed, result.Error);
        Assert.Empty(reported.Actions);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_refuse_les_trois_operations()
    {
        var action = (await AddAsync()).Value;
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var add = await AddAsync();
        var update = await new UpdateProblemAction(_problems, Guard, Check)
            .ExecuteAsync(_problem.Id, action.Id, new UpdateProblemActionInput(null, "x", null), CancellationToken.None);
        var close = await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.Retired, add.Error);
        Assert.Equal(VehicleErrors.Retired, update.Error);
        Assert.Equal(VehicleErrors.Retired, close.Error);
        Assert.Single(_problem.Actions);
        Assert.False(_problem.Actions[0].IsClosed);
    }

    [Fact]
    public async Task Acteur_type_ou_contrainte_inconnu_est_refuse()
    {
        var actor = await AddAsync(actorId: Guid.NewGuid());
        var type = await AddAsync(typeId: Guid.NewGuid());
        var constraint = await AddAsync(constraintId: Guid.NewGuid());

        Assert.Equal(ProblemErrors.ActorUnknown, actor.Error);
        Assert.Equal(ProblemErrors.ActionTypeUnknown, type.Error);
        Assert.Equal(ProblemErrors.ConstraintUnknown, constraint.Error);
        Assert.Empty(_problem.Actions);
    }

    [Fact]
    public async Task Acteur_type_ou_contrainte_inactif_est_refuse()
    {
        var actor = _actors.Seed("Ancien acteur", active: false);
        var type = _types.Seed("Ancien type", active: false);
        var constraint = _constraints.Seed("Ancienne contrainte", active: false);

        var byActor = await AddAsync(actorId: actor.Id);
        var byType = await AddAsync(typeId: type.Id);
        var byConstraint = await AddAsync(constraintId: constraint.Id);

        Assert.Equal("problem_actor.inactive", byActor.Error!.Code);
        Assert.Equal("problem_action_type.inactive", byType.Error!.Code);
        Assert.Equal("problem_constraint.inactive", byConstraint.Error!.Code);
        Assert.Empty(_problem.Actions);
    }

    [Fact]
    public async Task Une_intervention_d_un_autre_vehicule_ou_inconnue_est_refusee()
    {
        var other = _vehicles.Seed("EF-456-GH");
        var foreign = SeedIntervention(other);

        var otherVehicle = await AddAsync(interventionId: foreign.Id);
        var unknown = await AddAsync(interventionId: Guid.NewGuid());

        Assert.Equal(ProblemErrors.ActionInterventionInvalid, otherVehicle.Error);
        Assert.Equal(ProblemErrors.ActionInterventionInvalid, unknown.Error);
        Assert.Empty(_problem.Actions);
    }

    [Fact]
    public async Task Modifier_une_action_remplace_echeance_commentaire_et_intervention()
    {
        var action = (await AddAsync()).Value;
        var intervention = SeedIntervention(_vehicle);
        var due = FixedClock.DefaultToday.AddDays(2);

        var result = await new UpdateProblemAction(_problems, Guard, Check)
            .ExecuteAsync(_problem.Id, action.Id, new UpdateProblemActionInput(due, "Nouveau", intervention.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(due, result.Value.DueDate);
        Assert.Equal("Nouveau", result.Value.Comment);
        Assert.Equal(intervention.Id, result.Value.InterventionId);
    }

    [Fact]
    public async Task Modifier_refuse_une_intervention_d_un_autre_vehicule_une_action_inconnue_ou_cloturee()
    {
        var action = (await AddAsync(comment: "Avant")).Value;
        var foreign = SeedIntervention(_vehicles.Seed("EF-456-GH"));
        var update = new UpdateProblemAction(_problems, Guard, Check);

        var otherVehicle = await update.ExecuteAsync(_problem.Id, action.Id, new UpdateProblemActionInput(null, "Après", foreign.Id), CancellationToken.None);
        var unknown = await update.ExecuteAsync(_problem.Id, Guid.NewGuid(), new UpdateProblemActionInput(null, "Après", null), CancellationToken.None);
        var noProblem = await update.ExecuteAsync(Guid.NewGuid(), action.Id, new UpdateProblemActionInput(null, "Après", null), CancellationToken.None);
        await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);
        var closed = await update.ExecuteAsync(_problem.Id, action.Id, new UpdateProblemActionInput(null, "Après", null), CancellationToken.None);

        Assert.Equal(ProblemErrors.ActionInterventionInvalid, otherVehicle.Error);
        Assert.Equal(ProblemErrors.ActionNotFound, unknown.Error);
        Assert.Equal(ProblemErrors.NotFound, noProblem.Error);
        Assert.Equal(ProblemErrors.ActionAlreadyClosed, closed.Error);
        Assert.Equal("Avant", _problem.Actions[0].Comment);
    }

    [Fact]
    public async Task Cloturer_une_action_renseigne_la_date_de_cloture_et_laisse_le_probleme_analyse_s_il_reste_une_action_ouverte()
    {
        var action = (await AddAsync()).Value;
        await AddAsync();
        _clock.Now = _clock.Now.AddDays(3);

        var result = await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsClosed);
        Assert.Equal(_clock.Now, result.Value.ClosedAt);
        Assert.Equal(ProblemStatus.Analyzed, _problem.Status);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
    }

    [Fact]
    public async Task Cloturer_la_derniere_action_resout_le_probleme_non_immobilisant_sans_toucher_le_vehicule()
    {
        var action = (await AddAsync()).Value;
        _vehicle.ChangeStatus(VehicleStatus.Immobilized, _clock.Now);

        await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);

        Assert.Equal(ProblemStatus.Resolved, _problem.Status);
        Assert.Equal(VehicleStatus.Immobilized, _vehicle.Status);
    }

    [Fact]
    public async Task Modifier_une_action_cloturee_repond_deja_cloturee_avant_de_controler_l_intervention()
    {
        var action = (await AddAsync()).Value;
        await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);

        var result = await new UpdateProblemAction(_problems, Guard, Check)
            .ExecuteAsync(_problem.Id, action.Id, new UpdateProblemActionInput(null, "x", Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ProblemErrors.ActionAlreadyClosed, result.Error);
    }

    [Fact]
    public async Task Resoudre_le_dernier_probleme_immobilisant_remet_le_vehicule_en_service_en_une_sauvegarde()
    {
        var problem = NewProblem(_vehicle, analyzed: true, immobilized: true);
        _vehicle.ChangeStatus(VehicleStatus.Immobilized, _clock.Now);
        var action = (await AddAsync(problemId: problem.Id)).Value;
        var saves = _problems.SaveCount;

        var result = await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(problem.Id, action.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProblemStatus.Resolved, problem.Status);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
        Assert.Equal(saves + 1, _problems.SaveCount);
    }

    [Fact]
    public async Task Le_vehicule_reste_immobilise_tant_qu_un_autre_probleme_immobilisant_est_ouvert()
    {
        var first = NewProblem(_vehicle, analyzed: true, immobilized: true);
        var second = NewProblem(_vehicle, analyzed: true, immobilized: true);
        _vehicle.ChangeStatus(VehicleStatus.Immobilized, _clock.Now);
        var firstAction = (await AddAsync(problemId: first.Id)).Value;
        var secondAction = (await AddAsync(problemId: second.Id)).Value;
        var close = new CloseProblemAction(_problems, Guard, Sync, _clock);

        await close.ExecuteAsync(first.Id, firstAction.Id, CancellationToken.None);
        Assert.Equal(ProblemStatus.Resolved, first.Status);
        Assert.Equal(VehicleStatus.Immobilized, _vehicle.Status);

        await close.ExecuteAsync(second.Id, secondAction.Id, CancellationToken.None);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
    }

    [Fact]
    public async Task Ajouter_une_action_a_un_probleme_immobilisant_resolu_le_rouvre_et_immobilise_le_vehicule()
    {
        var problem = NewProblem(_vehicle, analyzed: true, immobilized: true);
        _vehicle.ChangeStatus(VehicleStatus.Immobilized, _clock.Now);
        var action = (await AddAsync(problemId: problem.Id)).Value;
        await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(problem.Id, action.Id, CancellationToken.None);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);

        var result = await AddAsync(problemId: problem.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProblemStatus.Analyzed, problem.Status);
        Assert.Equal(VehicleStatus.Immobilized, _vehicle.Status);
    }

    [Fact]
    public async Task Rouvrir_un_probleme_non_immobilisant_ne_change_pas_le_vehicule()
    {
        var action = (await AddAsync()).Value;
        await new CloseProblemAction(_problems, Guard, Sync, _clock).ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);
        Assert.Equal(ProblemStatus.Resolved, _problem.Status);

        await AddAsync();

        Assert.Equal(ProblemStatus.Analyzed, _problem.Status);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_n_est_jamais_modifie_par_la_synchronisation()
    {
        var problem = NewProblem(_vehicle, analyzed: true, immobilized: true);
        var action = (await AddAsync(problemId: problem.Id)).Value;
        problem.CloseAction(action.Id, _clock.Now);
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);
        var updatedAt = _vehicle.UpdatedAt;

        var resolved = await Sync.ApplyAsync(problem, ProblemStatus.Analyzed, _clock.Now, CancellationToken.None);
        var reopened = await Sync.ApplyAsync(problem, ProblemStatus.Resolved, _clock.Now, CancellationToken.None);

        Assert.Null(resolved);
        Assert.Null(reopened);
        Assert.Equal(VehicleStatus.Retired, _vehicle.Status);
        Assert.Equal(updatedAt, _vehicle.UpdatedAt);
    }

    [Fact]
    public async Task Cloturer_refuse_une_action_inconnue_ou_deja_cloturee()
    {
        var action = (await AddAsync()).Value;
        var close = new CloseProblemAction(_problems, Guard, Sync, _clock);
        await close.ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);

        var again = await close.ExecuteAsync(_problem.Id, action.Id, CancellationToken.None);
        var unknown = await close.ExecuteAsync(_problem.Id, Guid.NewGuid(), CancellationToken.None);
        var noProblem = await close.ExecuteAsync(Guid.NewGuid(), action.Id, CancellationToken.None);

        Assert.Equal(ProblemErrors.ActionAlreadyClosed, again.Error);
        Assert.Equal(ProblemErrors.ActionNotFound, unknown.Error);
        Assert.Equal(ProblemErrors.NotFound, noProblem.Error);
    }
}
