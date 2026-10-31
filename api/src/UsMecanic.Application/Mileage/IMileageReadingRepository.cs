using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Mileage;

/// <summary>Port de persistance des relevés kilométriques.</summary>
public interface IMileageReadingRepository
{
    Task<MileageReading?> GetAsync(Guid vehicleId, Guid readingId, CancellationToken cancellationToken);

    /// <summary>Relevés d'un véhicule, du plus récent au plus ancien (par date, puis par saisie).</summary>
    Task<IReadOnlyList<MileageReading>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>Relevés dont la date est comprise entre <paramref name="firstDay"/> et <paramref name="lastDay"/> inclus, d'un véhicule ou de tout le parc (filtre côté base).</summary>
    Task<IReadOnlyList<MileageReading>> ListInPeriodAsync(DateOnly firstDay, DateOnly lastDay, Guid? vehicleId, CancellationToken cancellationToken);

    /// <summary>Km actuel d'un véhicule (voir <c>CurrentMileageRule</c>), ou <c>null</c> sans relevé.</summary>
    Task<CurrentMileage?> GetCurrentAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>Km actuel de plusieurs véhicules en une requête ; les véhicules sans relevé sont absents.</summary>
    Task<IReadOnlyDictionary<Guid, CurrentMileage>> GetCurrentByVehicleAsync(IReadOnlyCollection<Guid> vehicleIds, CancellationToken cancellationToken);

    void Add(MileageReading reading);

    void Remove(MileageReading reading);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
