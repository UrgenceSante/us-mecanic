using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Port de persistance des problèmes.</summary>
public interface IProblemRepository
{
    /// <summary>Problèmes d'un véhicule, du plus récent au plus ancien.</summary>
    Task<IReadOnlyList<Problem>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>Problème suivi par le contexte de persistance (modifiable), ou <c>null</c> s'il n'existe pas.</summary>
    Task<Problem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(Problem problem);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
