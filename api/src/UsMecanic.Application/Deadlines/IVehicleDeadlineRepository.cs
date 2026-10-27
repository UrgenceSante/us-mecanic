using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Port de persistance des échéances de véhicule.</summary>
public interface IVehicleDeadlineRepository
{
    Task<VehicleDeadline?> GetAsync(Guid vehicleId, Guid deadlineId, CancellationToken cancellationToken);

    /// <summary>Échéances d'un véhicule, closes comprises.</summary>
    Task<IReadOnlyList<VehicleDeadline>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>Indique si le véhicule a déjà une échéance non close de ce type.</summary>
    Task<bool> ActiveExistsAsync(Guid vehicleId, Guid deadlineTypeId, CancellationToken cancellationToken);

    void Add(VehicleDeadline deadline);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
