using UsMecanic.Application.Problems;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Problems;

public class ListFleetProblemsTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private static readonly ProblemBlockingLevel[] ExpectedLevels =
    [
        ProblemBlockingLevel.Immobilizing,
        ProblemBlockingLevel.Immobilizing,
        ProblemBlockingLevel.Unanalyzed,
        ProblemBlockingLevel.Unanalyzed,
        ProblemBlockingLevel.OverdueAction,
        ProblemBlockingLevel.OverdueAction,
        ProblemBlockingLevel.Other,
    ];

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryProblemRepository _problems = new();
    private readonly InMemoryProblemReferenceRepository<ProblemNature> _natures = new();
    private readonly Vehicle _vehicle;

    public ListFleetProblemsTests() => _vehicle = _vehicles.Seed("AB-123-CD");

    private ListFleetProblems UseCase => new(_problems, _vehicles, _natures, new FixedClock().Calendar);

    private Problem Add(
        Vehicle vehicle,
        int daysAgo,
        bool analyzed = true,
        bool immobilized = false,
        int? dueInDays = null,
        bool closeAction = false,
        Guid? natureId = null,
        DateTimeOffset? recordedAt = null)
    {
        var at = recordedAt ?? Now;
        var problem = Problem.Create(vehicle.Id, Today.AddDays(-daysAgo), "Voyant", "D. Martin", Today, at).Value;
        if (analyzed)
        {
            problem.Analyze(natureId ?? Guid.NewGuid(), Guid.NewGuid(), "Analyse", immobilized, at);
            if (dueInDays is not null || closeAction)
            {
                var action = problem.AddAction(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), dueInDays is { } d ? Today.AddDays(d) : null, null, null, at).Value;
                if (closeAction)
                {
                    problem.CloseAction(action.Id, at);
                }
            }
        }

        _problems.Saved.Add(problem);
        return problem;
    }

    private async Task<IReadOnlyList<FleetProblemDto>> ListAsync(FleetProblemFilter? filter = null) =>
        await UseCase.ExecuteAsync(filter ?? new FleetProblemFilter(), CancellationToken.None);

    [Fact]
    public async Task Les_problemes_sont_classes_par_niveau_de_blocage_puis_du_plus_ancien_au_plus_recent()
    {
        var other = Add(_vehicle, daysAgo: 20);
        var overdueRecent = Add(_vehicle, daysAgo: 2, dueInDays: -1);
        var unanalyzedRecent = Add(_vehicle, daysAgo: 1, analyzed: false);
        var immobilizingRecent = Add(_vehicle, daysAgo: 1, immobilized: true);
        var overdueOld = Add(_vehicle, daysAgo: 9, dueInDays: -5);
        var unanalyzedOld = Add(_vehicle, daysAgo: 7, analyzed: false);
        var immobilizingOld = Add(_vehicle, daysAgo: 6, immobilized: true);

        var list = await ListAsync();

        Guid[] expectedIds = [immobilizingOld.Id, immobilizingRecent.Id, unanalyzedOld.Id, unanalyzedRecent.Id, overdueOld.Id, overdueRecent.Id, other.Id];
        Assert.Equal(expectedIds, list.Select(l => l.Problem.Id).ToList());
        Assert.Equal(ExpectedLevels, list.Select(l => l.BlockingLevel).ToList());
    }

    [Fact]
    public async Task A_egalite_de_date_de_signalement_le_plus_tot_saisi_passe_d_abord()
    {
        var later = Add(_vehicle, daysAgo: 3, recordedAt: Now.AddHours(2));
        var earlier = Add(_vehicle, daysAgo: 3, recordedAt: Now.AddHours(1));

        var list = await ListAsync();

        Guid[] expected = [earlier.Id, later.Id];
        Assert.Equal(expected, list.Select(l => l.Problem.Id).ToList());
    }

    [Fact]
    public async Task Une_action_close_en_retard_ou_une_echeance_aujourd_hui_ne_classe_pas_en_retard()
    {
        Add(_vehicle, daysAgo: 4, dueInDays: -2, closeAction: true);
        Add(_vehicle, daysAgo: 3, dueInDays: 0);
        Add(_vehicle, daysAgo: 2, dueInDays: 3);

        var list = await ListAsync(new FleetProblemFilter(ProblemStatus.Analyzed));

        Assert.Equal(2, list.Count);
        Assert.All(list, l => Assert.Equal(ProblemBlockingLevel.Other, l.BlockingLevel));
    }

    [Fact]
    public async Task Les_problemes_resolus_sont_exclus_par_defaut_et_renvoyes_si_le_statut_est_demande()
    {
        var open = Add(_vehicle, daysAgo: 2);
        var resolved = Add(_vehicle, daysAgo: 3, dueInDays: -1, closeAction: true);

        Assert.Equal(open.Id, Assert.Single(await ListAsync()).Problem.Id);
        Assert.Equal(resolved.Id, Assert.Single(await ListAsync(new FleetProblemFilter(ProblemStatus.Resolved))).Problem.Id);
    }

    [Fact]
    public async Task Les_filtres_statut_vehicule_et_nature_se_combinent()
    {
        var other = _vehicles.Seed("EF-456-GH");
        var nature = _natures.Seed("Mécanique");
        var wanted = Add(_vehicle, daysAgo: 1, natureId: nature.Id);
        Add(_vehicle, daysAgo: 2);
        Add(_vehicle, daysAgo: 3, analyzed: false);
        Add(other, daysAgo: 4, natureId: nature.Id);

        var byNature = await ListAsync(new FleetProblemFilter(NatureId: nature.Id));
        var byVehicle = await ListAsync(new FleetProblemFilter(VehicleId: other.Id));
        var combined = await ListAsync(new FleetProblemFilter(ProblemStatus.Analyzed, _vehicle.Id, nature.Id));
        var byStatus = await ListAsync(new FleetProblemFilter(ProblemStatus.Reported));

        Assert.Equal(2, byNature.Count);
        Assert.Single(byVehicle);
        Assert.Equal(wanted.Id, Assert.Single(combined).Problem.Id);
        Assert.Equal(_vehicle.Id, Assert.Single(byStatus).Vehicle.Id);
    }

    [Fact]
    public async Task Une_ligne_porte_le_vehicule_et_la_nature_sans_nature_avant_analyse()
    {
        var nature = _natures.Seed("Mécanique");
        Add(_vehicle, daysAgo: 1, natureId: nature.Id);
        Add(_vehicle, daysAgo: 2, analyzed: false);

        var list = await ListAsync();

        Assert.Equal(new FleetProblemNatureDto(nature.Id, "Mécanique"), list.Single(l => l.Problem.Analysis is not null).Nature);
        Assert.Null(list.Single(l => l.Problem.Analysis is null).Nature);
        Assert.All(list, l => Assert.Equal(new FleetProblemVehicleDto(_vehicle.Id, "AB-123-CD"), l.Vehicle));
    }

    [Fact]
    public async Task Les_problemes_d_un_vehicule_sorti_du_parc_sont_masques()
    {
        var retired = _vehicles.Seed("EF-456-GH");
        Add(retired, daysAgo: 1, immobilized: true);
        var kept = Add(_vehicle, daysAgo: 2);
        retired.ChangeStatus(VehicleStatus.Retired, Now);

        var list = await ListAsync();

        Assert.Equal(kept.Id, Assert.Single(list).Problem.Id);
    }

    [Fact]
    public async Task Sans_probleme_la_liste_est_vide()
    {
        Assert.Empty(await ListAsync());
    }
}
