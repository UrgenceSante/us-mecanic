using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Problems;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Calcule les alertes de plusieurs véhicules en lot (nombre de requêtes constant, sans N+1) : échéances dépassées
/// selon le statut du Domain, et problèmes immobilisants non résolus.
/// </summary>
public sealed class VehicleAlertsLookup(
    IVehicleDeadlineRepository deadlines,
    IDeadlineTypeRepository deadlineTypes,
    IDeadlineThresholdsRepository thresholds,
    IProblemRepository problems,
    BusinessCalendar calendar)
{
    public async Task<IReadOnlyList<VehicleDto>> WithAlertsAsync(IReadOnlyList<VehicleDto> vehicles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(vehicles);

        if (vehicles.Count == 0)
        {
            return vehicles;
        }

        var ids = vehicles.Select(v => v.Id).ToHashSet();
        var overdue = await CountOverdueAsync(vehicles, ids, cancellationToken);
        var immobilizing = (await problems.ListFleetAsync(new FleetProblemFilter(), cancellationToken))
            .Where(p => p.Status != ProblemStatus.Resolved && p.Immobilized == true && ids.Contains(p.VehicleId))
            .Select(p => p.VehicleId)
            .ToHashSet();

        return [.. vehicles.Select(v => v with
        {
            // Un véhicule sorti du parc n'a plus d'alerte : ses échéances et problèmes ne sont plus suivis.
            Alerts = v.Status == VehicleStatus.Retired
                ? new VehicleAlertsDto(0, false)
                : new VehicleAlertsDto(overdue.GetValueOrDefault(v.Id), immobilizing.Contains(v.Id)),
        })];
    }

    private async Task<Dictionary<Guid, int>> CountOverdueAsync(IReadOnlyList<VehicleDto> vehicles, HashSet<Guid> ids, CancellationToken cancellationToken)
    {
        var active = (await deadlines.ListActiveAsync(null, null, cancellationToken)).Where(d => ids.Contains(d.VehicleId)).ToList();
        if (active.Count == 0)
        {
            return [];
        }

        var typesById = (await deadlineTypes.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(t => t.Id);
        var global = await thresholds.GetAsync(cancellationToken);
        var currentKm = vehicles.ToDictionary(v => v.Id, v => v.CurrentMileage);
        var today = calendar.Today;

        return active
            .Where(d => typesById.TryGetValue(d.DeadlineTypeId, out var type)
                && d.StatusAt(currentKm[d.VehicleId], today, DueSoonLimits.For(type, global)) == DeadlineStatus.Overdue)
            .GroupBy(d => d.VehicleId)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
