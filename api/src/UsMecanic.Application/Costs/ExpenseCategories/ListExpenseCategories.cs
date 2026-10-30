namespace UsMecanic.Application.Costs.ExpenseCategories;

/// <summary>Liste les catégories. Les catégories désactivées ne sont plus proposées, sauf demande explicite.</summary>
public sealed class ListExpenseCategories(IExpenseCategoryRepository repository)
{
    public async Task<IReadOnlyList<ExpenseCategoryDto>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var expenseCategories = await repository.ListAsync(includeInactive, cancellationToken);
        return [.. expenseCategories.Select(ExpenseCategoryDto.From)];
    }
}
