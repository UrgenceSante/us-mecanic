using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Mileage;

/// <summary>Port de persistance des relevés kilométriques.</summary>
public interface IMileageReadingRepository
{
    Task<MileageReading?> GetAsync(Guid vehicleId, Guid readingId, CancellationToken cancellationToken);

    /// <summary>Relevés d'un véhicule, du plus récent au plus ancien (par date, puis par saisie).</summary>
    Task<IReadOnlyList<MileageReading>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    void Add(MileageReading reading);

    void Remove(MileageReading reading);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
