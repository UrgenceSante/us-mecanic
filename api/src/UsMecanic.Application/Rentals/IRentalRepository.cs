using UsMecanic.Domain.Rentals;

namespace UsMecanic.Application.Rentals;

/// <summary>Port de persistance des locations.</summary>
public interface IRentalRepository
{
    Task<Rental?> GetAsync(Guid vehicleId, Guid rentalId, CancellationToken cancellationToken);

    /// <summary>Locations d'un véhicule, de la plus récente à la plus ancienne (par date de début).</summary>
    Task<IReadOnlyList<Rental>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>
    /// Locations qui couvrent le jour, pour les véhicules demandés ou, sans filtre, pour tout le parc
    /// (une seule requête, quel que soit le nombre de véhicules).
    /// </summary>
    Task<IReadOnlyList<Rental>> ListCoveringAsync(DateOnly day, IReadOnlyCollection<Guid>? vehicleIds, CancellationToken cancellationToken);

    /// <summary>La société loue ce véhicule en ce moment ou à partir d'une date future (location non terminée au jour donné).</summary>
    Task<bool> HasCurrentOrUpcomingAsync(Guid vehicleId, Guid tenantCompanyId, DateOnly today, CancellationToken cancellationToken);

    void Add(Rental rental);

    void Remove(Rental rental);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
