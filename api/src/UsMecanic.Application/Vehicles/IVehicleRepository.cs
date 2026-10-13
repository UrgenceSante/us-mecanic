using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>Port de persistance des véhicules.</summary>
public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Vehicle?> GetByPlateAsync(LicensePlate licensePlate, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vehicle>> GetByPlatesAsync(IReadOnlyCollection<LicensePlate> licensePlates, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vehicle>> ListAsync(VehicleFilter filter, CancellationToken cancellationToken);

    void Add(Vehicle vehicle);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
