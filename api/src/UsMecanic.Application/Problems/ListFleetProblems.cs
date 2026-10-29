using UsMecanic.Application.Problems.References;
using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Problèmes du parc, du plus au moins bloquant (voir <see cref="ProblemBlockingLevel"/>) puis du plus ancien au plus
/// récent (date de signalement, puis date de saisie). Par défaut les problèmes résolus sont exclus ; les problèmes d'un
/// véhicule sorti du parc sont masqués (comme ses échéances sur le tableau de bord). Le volume de problèmes ouverts
/// est faible : ils sont chargés avec leurs actions, puis classés en mémoire, le niveau étant une règle du Domain.
/// </summary>
public sealed class ListFleetProblems(
    IProblemRepository problems,
    IVehicleRepository vehicles,
    IProblemReferenceRepository<ProblemNature> natures,
    BusinessCalendar calendar)
{
    public async Task<IReadOnlyList<FleetProblemDto>> ExecuteAsync(FleetProblemFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var loaded = await problems.ListFleetAsync(filter, cancellationToken);
        if (loaded.Count == 0)
        {
            return [];
        }

        var fleet = (await vehicles.ListAsync(new VehicleFilter(), cancellationToken))
            .Where(v => v.Status != VehicleStatus.Retired)
            .ToDictionary(v => v.Id);
        var natureNames = (await natures.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(n => n.Id, n => n.Name);
        var today = calendar.Today;

        return [.. loaded
            .Where(p => fleet.ContainsKey(p.VehicleId))
            .Select(p => (Problem: p, Level: p.BlockingLevelAt(today)))
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Problem.Date)
            .ThenBy(x => x.Problem.RecordedAt)
            .ThenBy(x => x.Problem.Id)
            .Select(x => ToDto(x.Problem, x.Level, fleet[x.Problem.VehicleId], natureNames))];
    }

    private static FleetProblemDto ToDto(Problem problem, ProblemBlockingLevel level, Vehicle vehicle, Dictionary<Guid, string> natureNames)
    {
        FleetProblemNatureDto? nature = problem.NatureId is { } natureId && natureNames.TryGetValue(natureId, out var name)
            ? new FleetProblemNatureDto(natureId, name)
            : null;
        return new FleetProblemDto(ProblemDto.From(problem), new FleetProblemVehicleDto(vehicle.Id, vehicle.LicensePlate.Value), nature, level);
    }
}
