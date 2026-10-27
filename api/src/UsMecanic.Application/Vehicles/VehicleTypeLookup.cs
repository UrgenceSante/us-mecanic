using UsMecanic.Application.Vehicles.Types;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Relie les véhicules à leur type : contrôle qu'un type peut être attribué et fournit le nom du type
/// aux DTO (y compris pour un type désactivé, qui reste affiché sur les véhicules qui l'ont).
/// </summary>
public sealed class VehicleTypeLookup(IVehicleTypeRepository types)
{
    /// <summary>
    /// Un type doit exister et être actif pour être attribué ; un véhicule peut en revanche conserver
    /// son type actuel même désactivé. Renvoie <c>null</c> si l'attribution est permise.
    /// </summary>
    public async Task<DomainError?> CheckAssignableAsync(Guid? typeId, Guid? currentTypeId, CancellationToken cancellationToken)
    {
        if (typeId is null || typeId == currentTypeId)
        {
            return null;
        }

        var type = await types.GetByIdAsync(typeId.Value, cancellationToken);
        if (type is null)
        {
            return VehicleTypeErrors.Unknown;
        }

        return type.IsActive ? null : VehicleTypeErrors.Inactive;
    }

    public async Task<VehicleDto> ToDtoAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        var type = vehicle.TypeId is { } id ? await types.GetByIdAsync(id, cancellationToken) : null;
        return VehicleDto.From(vehicle, type?.Name);
    }

    public async Task<IReadOnlyList<VehicleDto>> ToDtosAsync(IReadOnlyList<Vehicle> vehicles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(vehicles);

        var names = (await types.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(t => t.Id, t => t.Name);
        return [.. vehicles.Select(v => VehicleDto.From(v, v.TypeId is { } id ? names.GetValueOrDefault(id) : null))];
    }
}
