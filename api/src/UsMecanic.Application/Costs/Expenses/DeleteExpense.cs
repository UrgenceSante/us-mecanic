using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>Supprime une dépense manuelle ; une dépense issue d'un connecteur est refusée. Renvoie son identifiant.</summary>
public sealed class DeleteExpense(MileageVehicleGuard guard, IExpenseRepository expenses)
{
    public async Task<Result<Guid>> ExecuteAsync(Guid vehicleId, Guid expenseId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<Guid>.Failure(error);
        }

        var expense = await expenses.GetAsync(vehicleId, expenseId, cancellationToken);
        if (expense is null)
        {
            return Result<Guid>.Failure(ExpenseErrors.NotFound);
        }

        if (expense.Source != ExpenseSource.Manual)
        {
            return Result<Guid>.Failure(ExpenseErrors.NotManual);
        }

        expenses.Remove(expense);
        await expenses.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(expense.Id);
    }
}
