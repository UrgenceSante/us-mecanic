using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles.Types;

/// <summary>Port de persistance des types de véhicule.</summary>
public interface IVehicleTypeRepository
{
    Task<VehicleType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Types triés par nom ; les types désactivés ne sont inclus que sur demande.</summary>
    Task<IReadOnlyList<VehicleType>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    /// <summary>Indique si un autre type (hors <paramref name="exceptId"/>) porte ce nom, sans tenir compte de la casse.</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    void Add(VehicleType vehicleType);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
