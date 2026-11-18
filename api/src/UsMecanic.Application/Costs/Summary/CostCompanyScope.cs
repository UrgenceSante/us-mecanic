using UsMecanic.Application.Rentals;
using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Costs.Summary;

/// <summary>Lignes de coût et relevés restreints aux filtres société.</summary>
public sealed record CostScopeResult(IReadOnlyList<CostLine> Lines, IReadOnlyList<MileageReading> Readings);

/// <summary>
/// Filtres société de la synthèse (R7 et R8), en une requête de véhicules et une requête de locations au plus :
/// le propriétaire est le propriétaire actuel quelle que soit la date ; le locataire est celui de la location
/// qui couvre la date de la dépense, et les mensualités d'emprunt ne lui sont jamais rattachées.
/// </summary>
public sealed class CostCompanyScope(IVehicleRepository vehicles, IRentalRepository rentals)
{
    public async Task<CostScopeResult> ApplyAsync(
        IReadOnlyList<CostLine> lines,
        IReadOnlyList<MileageReading> readings,
        DateOnly firstDay,
        DateOnly lastDay,
        Guid? ownerCompanyId,
        Guid? lesseeCompanyId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(readings);

        var scopedLines = lines;
        var scopedReadings = readings;
        if (ownerCompanyId is { } owner)
        {
            var owned = (await vehicles.ListAsync(new VehicleFilter(), cancellationToken))
                .Where(v => v.OwnerCompanyId == owner)
                .Select(v => v.Id)
                .ToHashSet();
            scopedLines = [.. scopedLines.Where(l => l.VehicleId is { } id && owned.Contains(id))];
            scopedReadings = [.. scopedReadings.Where(r => owned.Contains(r.VehicleId))];
        }

        if (lesseeCompanyId is { } lessee)
        {
            var leases = (await rentals.ListOverlappingAsync(firstDay, lastDay, cancellationToken))
                .Where(r => r.TenantCompanyId == lessee)
                .ToLookup(r => r.VehicleId);
            scopedLines = [.. scopedLines.Where(l => l.Family != CostFamily.Loan && l.VehicleId is { } id && leases[id].Any(r => r.Covers(l.Date)))];
        }

        return new CostScopeResult(scopedLines, scopedReadings);
    }
}
