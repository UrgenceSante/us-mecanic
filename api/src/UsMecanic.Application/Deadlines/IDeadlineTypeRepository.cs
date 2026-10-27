using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Port de persistance des types d'échéance.</summary>
public interface IDeadlineTypeRepository
{
    Task<DeadlineType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Types triés par nom ; les types désactivés ne sont inclus que sur demande.</summary>
    Task<IReadOnlyList<DeadlineType>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    /// <summary>Indique si un autre type (hors <paramref name="exceptId"/>) porte ce nom, sans tenir compte de la casse.</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    void Add(DeadlineType deadlineType);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
