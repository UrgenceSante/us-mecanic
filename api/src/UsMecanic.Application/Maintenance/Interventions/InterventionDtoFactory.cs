using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Maintenance.Garages;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>
/// Construit les DTO du carnet en un nombre constant de requêtes (garages, échéances du véhicule, types) :
/// pas de N+1, quel que soit le nombre d'interventions.
/// </summary>
public sealed class InterventionDtoFactory(
    IGarageRepository garages,
    IVehicleDeadlineRepository deadlines,
    IDeadlineTypeRepository types)
{
    public async Task<IReadOnlyList<InterventionDto>> BuildAsync(
        Guid vehicleId, IReadOnlyList<Intervention> interventions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(interventions);

        if (interventions.Count == 0)
        {
            return [];
        }

        var garagesById = (await garages.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(g => g.Id);
        var typesById = (await types.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(t => t.Id);
        var deadlinesById = (await deadlines.ListByVehicleAsync(vehicleId, cancellationToken)).ToDictionary(d => d.Id);

        return [.. interventions.Select(intervention =>
        {
            var garage = intervention.GarageId is { } garageId && garagesById.TryGetValue(garageId, out var found)
                ? new InterventionGarageDto(found.Id, found.Name)
                : null;
            var done = intervention.Deadlines.Select(link =>
            {
                var typeName = deadlinesById.TryGetValue(link.VehicleDeadlineId, out var deadline)
                    && typesById.TryGetValue(deadline.DeadlineTypeId, out var type)
                        ? type.Name
                        : null;
                return new InterventionDeadlineDto(link.VehicleDeadlineId, typeName);
            });

            return new InterventionDto(
                intervention.Id,
                intervention.VehicleId,
                intervention.Date,
                intervention.Kilometers,
                garage,
                intervention.Work,
                intervention.RecordedAt,
                [.. done.OrderBy(d => d.TypeName, StringComparer.OrdinalIgnoreCase)]);
        })];
    }
}
