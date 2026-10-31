using UsMecanic.Application.Costs.ExpenseCategories;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>Construit les DTO avec le nom des catégories en une seule requête, quel que soit le nombre de dépenses.</summary>
public sealed class ExpenseDtoFactory(IExpenseCategoryRepository categories, BusinessCalendar calendar)
{
    public async Task<IReadOnlyList<ExpenseDto>> BuildAsync(IReadOnlyList<Expense> expenses, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(expenses);

        if (expenses.Count == 0)
        {
            return [];
        }

        var names = (await categories.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(c => c.Id, c => c.Name);
        var today = calendar.Today;
        return
        [
            .. expenses.Select(e => new ExpenseDto(
                e.Id,
                e.VehicleId,
                e.Date,
                e.Family,
                e.CategoryId,
                e.CategoryId is { } id && names.TryGetValue(id, out var name) ? name : null,
                e.Amount,
                e.Label,
                e.Source,
                e.RecordedAt,
                e.Date > today)),
        ];
    }
}
