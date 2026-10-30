using UsMecanic.Application.Costs.ExpenseCategories;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>
/// Modifie une dépense manuelle (une dépense issue d'un connecteur est refusée). Une catégorie inchangée reste acceptée
/// même si elle a été désactivée depuis : seule une nouvelle catégorie doit exister et être active.
/// </summary>
public sealed class UpdateExpense(
    MileageVehicleGuard guard,
    IExpenseCategoryRepository categories,
    IExpenseRepository expenses,
    ExpenseDtoFactory factory,
    BusinessCalendar calendar)
{
    public async Task<Result<ExpenseDto>> ExecuteAsync(Guid vehicleId, Guid expenseId, ExpenseInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<ExpenseDto>.Failure(error);
        }

        var expense = await expenses.GetAsync(vehicleId, expenseId, cancellationToken);
        if (expense is null)
        {
            return Result<ExpenseDto>.Failure(ExpenseErrors.NotFound);
        }

        if (expense.Source != ExpenseSource.Manual)
        {
            return Result<ExpenseDto>.Failure(ExpenseErrors.NotManual);
        }

        if (input.Family != expense.Family)
        {
            return Result<ExpenseDto>.Failure(ExpenseErrors.FamilyNotManual);
        }

        if (input.CategoryId != expense.CategoryId)
        {
            var categoryError = await AddExpense.CheckCategoryAsync(categories, input.CategoryId, cancellationToken);
            if (categoryError is not null)
            {
                return Result<ExpenseDto>.Failure(categoryError);
            }
        }

        var updated = expense.Update(input.Date, input.CategoryId, input.Amount, input.Label, calendar.Today);
        if (updated.IsFailure)
        {
            return Result<ExpenseDto>.Failure(updated.Error!);
        }

        await expenses.SaveChangesAsync(cancellationToken);
        return Result<ExpenseDto>.Success((await factory.BuildAsync([expense], cancellationToken))[0]);
    }
}
