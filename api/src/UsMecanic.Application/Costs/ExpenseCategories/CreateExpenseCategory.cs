using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.ExpenseCategories;

/// <summary>Ajoute une catégorie. Le nom est obligatoire et unique (sans tenir compte de la casse).</summary>
public sealed class CreateExpenseCategory(IExpenseCategoryRepository repository)
{
    public async Task<Result<ExpenseCategoryDto>> ExecuteAsync(string? name, CancellationToken cancellationToken)
    {
        var expenseCategory = ExpenseCategory.Create(name);
        if (expenseCategory.IsFailure)
        {
            return Result<ExpenseCategoryDto>.Failure(expenseCategory.Error!);
        }

        if (await repository.NameExistsAsync(expenseCategory.Value.Name, null, cancellationToken))
        {
            return Result<ExpenseCategoryDto>.Failure(ExpenseCategoryErrors.NameAlreadyExists);
        }

        repository.Add(expenseCategory.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<ExpenseCategoryDto>.Success(ExpenseCategoryDto.From(expenseCategory.Value));
    }
}
