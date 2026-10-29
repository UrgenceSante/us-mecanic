using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems.References;

/// <summary>Port de persistance d'un référentiel des problèmes (natures, éléments, acteurs, types d'action, contraintes).</summary>
public interface IProblemReferenceRepository<T>
    where T : ProblemReference, IProblemReference<T>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Valeurs triées par libellé ; les valeurs désactivées ne sont incluses que sur demande.</summary>
    Task<IReadOnlyList<T>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    /// <summary>Indique si une autre valeur (hors <paramref name="exceptId"/>) porte ce libellé, sans tenir compte de la casse.</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    void Add(T item);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
