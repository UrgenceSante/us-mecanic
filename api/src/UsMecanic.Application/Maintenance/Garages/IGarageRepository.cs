using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Garages;

/// <summary>Port de persistance des garages.</summary>
public interface IGarageRepository
{
    Task<Garage?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Garages triés par nom ; les garages désactivés ne sont inclus que sur demande.</summary>
    Task<IReadOnlyList<Garage>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    /// <summary>Indique si un autre garage (hors <paramref name="exceptId"/>) porte ce nom, sans tenir compte de la casse.</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    void Add(Garage garage);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
