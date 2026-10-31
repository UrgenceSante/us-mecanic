using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>Dépenses d'un véhicule, de la plus récente à la plus ancienne.</summary>
public sealed class ListExpenses(MileageVehicleGuard guard, IExpenseRepository expenses, ExpenseDtoFactory factory)
{
    public async Task<Result<IReadOnlyList<ExpenseDto>>> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<ExpenseDto>>.Failure(error);
        }

        var list = await expenses.ListByVehicleAsync(vehicleId, cancellationToken);
        return Result<IReadOnlyList<ExpenseDto>>.Success(await factory.BuildAsync(list, cancellationToken));
    }
}
