using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Loans;

/// <summary>Port de persistance des emprunts.</summary>
public interface ILoanRepository
{
    Task<Loan?> GetAsync(Guid vehicleId, Guid loanId, CancellationToken cancellationToken);

    /// <summary>Emprunts d'un véhicule, du plus récent au plus ancien (par date de début, puis par saisie).</summary>
    Task<IReadOnlyList<Loan>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>
    /// Emprunts dont la date de début n'est pas postérieure à <paramref name="to"/>, pour un véhicule ou, sans filtre,
    /// pour tout le parc. Les emprunts déjà terminés avant la période sont écartés par l'appelant (la fin se déduit de la durée).
    /// </summary>
    Task<IReadOnlyList<Loan>> ListStartedBeforeAsync(DateOnly to, Guid? vehicleId, CancellationToken cancellationToken);

    void Add(Loan loan);

    void Remove(Loan loan);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
