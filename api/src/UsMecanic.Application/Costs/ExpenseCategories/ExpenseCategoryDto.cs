using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.ExpenseCategories;

public sealed record ExpenseCategoryDto(Guid Id, string Name, bool IsActive)
{
    public static ExpenseCategoryDto From(ExpenseCategory expenseCategory)
    {
        ArgumentNullException.ThrowIfNull(expenseCategory);

        return new(expenseCategory.Id, expenseCategory.Name, expenseCategory.IsActive);
    }
}
