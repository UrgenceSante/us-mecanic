using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>
/// Construit les DTO d'échéances avec leur statut : km actuel = relevé le plus récent par date,
/// date du jour du calendrier métier, seuils du type sinon globaux.
/// </summary>
public sealed class VehicleDeadlineDtoFactory(
    IDeadlineTypeRepository types,
    IDeadlineThresholdsRepository thresholds,
    IMileageReadingRepository readings,
    BusinessCalendar calendar)
{
    public async Task<IReadOnlyList<VehicleDeadlineDto>> BuildAsync(
        Guid vehicleId, IReadOnlyList<VehicleDeadline> deadlines, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(deadlines);

        var typesById = (await types.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(t => t.Id);
        var global = await thresholds.GetAsync(cancellationToken);
        var vehicleReadings = await readings.ListByVehicleAsync(vehicleId, cancellationToken);
        int? currentKm = vehicleReadings.Count > 0 ? vehicleReadings[0].Kilometers : null;
        var today = calendar.Today;

        return [.. deadlines.Select(deadline =>
        {
            var type = typesById.GetValueOrDefault(deadline.DeadlineTypeId);
            var status = type is null ? null : deadline.StatusAt(currentKm, today, DueSoonLimits.For(type, global));
            return VehicleDeadlineDto.From(deadline, type?.Name, status);
        })];
    }
}
