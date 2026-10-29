using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Port de persistance des problèmes.</summary>
public interface IProblemRepository
{
    /// <summary>Problèmes d'un véhicule, du plus récent au plus ancien.</summary>
    Task<IReadOnlyList<Problem>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    void Add(Problem problem);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
