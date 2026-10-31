using UsMecanic.Application.Costs.ExpenseCategories;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>Ajoute une dépense ponctuelle saisie à la main : catégorie existante et active obligatoire.</summary>
public sealed class AddExpense(
    MileageVehicleGuard guard,
    IExpenseCategoryRepository categories,
    IExpenseRepository expenses,
    ExpenseDtoFactory factory,
    TimeProvider clock)
{
    public async Task<Result<ExpenseDto>> ExecuteAsync(Guid vehicleId, ExpenseInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<ExpenseDto>.Failure(error);
        }

        var created = Expense.CreateManual(
            vehicleId, input.Family, input.Date, input.CategoryId, input.Amount, input.Label, clock.GetUtcNow());
        if (created.IsFailure)
        {
            return Result<ExpenseDto>.Failure(created.Error!);
        }

        var categoryError = await CheckCategoryAsync(categories, created.Value.CategoryId, cancellationToken);
        if (categoryError is not null)
        {
            return Result<ExpenseDto>.Failure(categoryError);
        }

        expenses.Add(created.Value);
        await expenses.SaveChangesAsync(cancellationToken);
        return Result<ExpenseDto>.Success((await factory.BuildAsync([created.Value], cancellationToken))[0]);
    }

    /// <summary>Catégorie choisie : elle doit exister et être active.</summary>
    internal static async Task<DomainError?> CheckCategoryAsync(
        IExpenseCategoryRepository categories, Guid? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is not { } id)
        {
            return ExpenseErrors.CategoryRequired;
        }

        var category = await categories.GetByIdAsync(id, cancellationToken);
        return category is null ? ExpenseCategoryErrors.Unknown : category.GetSelectionError();
    }
}
