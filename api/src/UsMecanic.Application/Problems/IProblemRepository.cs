using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Port de persistance des problèmes.</summary>
public interface IProblemRepository
{
    /// <summary>Problèmes d'un véhicule, du plus récent au plus ancien.</summary>
    Task<IReadOnlyList<Problem>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>
    /// Problèmes de tout le parc avec leurs actions, sans ordre garanti : ceux qui ne sont pas résolus, ou ceux du statut
    /// demandé, restreints au véhicule et à la nature demandés.
    /// </summary>
    Task<IReadOnlyList<Problem>> ListFleetAsync(FleetProblemFilter filter, CancellationToken cancellationToken);

    /// <summary>Problème suivi par le contexte de persistance (modifiable), ou <c>null</c> s'il n'existe pas.</summary>
    Task<Problem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Indique si le véhicule a un autre problème immobilisant non résolu que <paramref name="exceptProblemId"/>.
    /// </summary>
    Task<bool> HasOpenImmobilizingAsync(Guid vehicleId, Guid exceptProblemId, CancellationToken cancellationToken);

    void Add(Problem problem);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
