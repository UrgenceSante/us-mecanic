using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Mileage;
using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Types;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Dashboard;

/// <summary>
/// Échéances non closes, proches ou dépassées, des véhicules du parc (immobilisés inclus, sortis du parc exclus).
/// Tri : dépassées d'abord, puis date d'échéance croissante, puis km restants croissants.
/// Nombre de requêtes constant : tout est chargé en lot et le statut est calculé en mémoire.
/// </summary>
public sealed class ListDashboardDeadlines(
    IVehicleDeadlineRepository deadlines,
    IVehicleRepository vehicles,
    IVehicleTypeRepository vehicleTypes,
    IDeadlineTypeRepository deadlineTypes,
    IDeadlineThresholdsRepository thresholds,
    IMileageReadingRepository readings,
    BusinessCalendar calendar)
{
    public async Task<IReadOnlyList<DashboardDeadlineDto>> ExecuteAsync(DashboardDeadlineFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var active = await deadlines.ListActiveAsync(filter.VehicleId, filter.DeadlineTypeId, cancellationToken);
        if (active.Count == 0)
        {
            return [];
        }

        var fleet = (await vehicles.ListAsync(new VehicleFilter(TypeId: filter.VehicleTypeId), cancellationToken))
            .Where(v => v.Status != VehicleStatus.Retired)
            .ToDictionary(v => v.Id);
        var typesById = (await deadlineTypes.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(t => t.Id);
        var vehicleTypeNames = (await vehicleTypes.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(t => t.Id, t => t.Name);
        var global = await thresholds.GetAsync(cancellationToken);
        var kept = active.Where(d => fleet.ContainsKey(d.VehicleId)).ToList();
        var currents = await readings.GetCurrentByVehicleAsync([.. kept.Select(d => d.VehicleId).Distinct()], cancellationToken);
        var today = calendar.Today;

        var lines = new List<DashboardDeadlineDto>();
        foreach (var deadline in kept)
        {
            if (!typesById.TryGetValue(deadline.DeadlineTypeId, out var type))
            {
                continue;
            }

            int? currentKm = currents.TryGetValue(deadline.VehicleId, out var current) ? current.Kilometers : null;
            var status = deadline.StatusAt(currentKm, today, DueSoonLimits.For(type, global));
            if (status is not { } value || value == DeadlineStatus.UpToDate || (filter.Status is { } wanted && wanted != value))
            {
                continue;
            }

            var vehicle = fleet[deadline.VehicleId];
            var typeName = vehicle.TypeId is { } typeId ? vehicleTypeNames.GetValueOrDefault(typeId) : null;
            lines.Add(new DashboardDeadlineDto(
                deadline.Id,
                new DashboardVehicleDto(vehicle.Id, vehicle.LicensePlate.Value, vehicle.InternalNumber, vehicle.TypeId, typeName),
                new DashboardDeadlineTypeDto(type.Id, type.Name),
                value,
                deadline.DueDate,
                deadline.DueKm,
                currentKm));
        }

        return [.. lines
            .OrderByDescending(l => l.Status)
            .ThenBy(l => l.DueDate ?? DateOnly.MaxValue)
            .ThenBy(l => l.DueKm - l.CurrentKm ?? int.MaxValue)
            .ThenBy(l => l.Vehicle.LicensePlate, StringComparer.Ordinal)
            .ThenBy(l => l.DeadlineType.Name, StringComparer.OrdinalIgnoreCase)];
    }
}
