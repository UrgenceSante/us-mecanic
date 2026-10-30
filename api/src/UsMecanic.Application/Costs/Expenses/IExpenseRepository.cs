using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>Port de persistance des dépenses.</summary>
public interface IExpenseRepository
{
    /// <summary>Dépenses d'un véhicule, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<Expense>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>Dépense du véhicule, suivie par le contexte (pour la modifier ou la supprimer).</summary>
    Task<Expense?> GetAsync(Guid vehicleId, Guid expenseId, CancellationToken cancellationToken);

    void Add(Expense expense);

    void Remove(Expense expense);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
