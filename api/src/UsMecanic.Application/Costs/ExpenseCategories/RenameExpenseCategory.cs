using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.ExpenseCategories;

/// <summary>Renomme une catégorie. Le nouveau nom doit rester unique.</summary>
public sealed class RenameExpenseCategory(IExpenseCategoryRepository repository)
{
    public async Task<Result<ExpenseCategoryDto>> ExecuteAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        var normalized = ExpenseCategory.NormalizeName(name);
        if (normalized.IsFailure)
        {
            return Result<ExpenseCategoryDto>.Failure(normalized.Error!);
        }

        var expenseCategory = await repository.GetByIdAsync(id, cancellationToken);
        if (expenseCategory is null)
        {
            return Result<ExpenseCategoryDto>.Failure(ExpenseCategoryErrors.NotFound);
        }

        if (await repository.NameExistsAsync(normalized.Value, id, cancellationToken))
        {
            return Result<ExpenseCategoryDto>.Failure(ExpenseCategoryErrors.NameAlreadyExists);
        }

        expenseCategory.Rename(normalized.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<ExpenseCategoryDto>.Success(ExpenseCategoryDto.From(expenseCategory));
    }
}
