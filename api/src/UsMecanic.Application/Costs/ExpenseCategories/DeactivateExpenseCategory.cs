using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.ExpenseCategories;

/// <summary>
/// Désactive une catégorie : elle n'est plus proposée mais reste affichée sur les dépenses qui l'ont.
/// Il n'existe volontairement aucune suppression de catégorie.
/// </summary>
public sealed class DeactivateExpenseCategory(IExpenseCategoryRepository repository)
{
    public async Task<Result<ExpenseCategoryDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var expenseCategory = await repository.GetByIdAsync(id, cancellationToken);
        if (expenseCategory is null)
        {
            return Result<ExpenseCategoryDto>.Failure(ExpenseCategoryErrors.NotFound);
        }

        expenseCategory.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return Result<ExpenseCategoryDto>.Success(ExpenseCategoryDto.From(expenseCategory));
    }
}
