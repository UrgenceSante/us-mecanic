using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Port de persistance des seuils globaux (une seule ligne).</summary>
public interface IDeadlineThresholdsRepository
{
    /// <summary>Renvoie les seuils globaux, avec leurs valeurs par défaut s'ils n'existent pas encore.</summary>
    Task<DeadlineThresholds> GetAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
