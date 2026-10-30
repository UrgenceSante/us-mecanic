using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.ExpenseCategories;

/// <summary>Port de persistance des catégories.</summary>
public interface IExpenseCategoryRepository
{
    Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Catégories triées par nom ; les catégories désactivées ne sont incluses que sur demande.</summary>
    Task<IReadOnlyList<ExpenseCategory>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    /// <summary>Indique si une autre catégorie (hors <paramref name="exceptId"/>) porte ce nom, sans tenir compte de la casse.</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    void Add(ExpenseCategory expenseCategory);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
