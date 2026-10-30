using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Costs.ExpenseCategories;
using UsMecanic.Domain.Costs;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Costs;

internal sealed class ExpenseCategoryRepository(UsMecanicDbContext db) : IExpenseCategoryRepository
{
    public Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.ExpenseCategories.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ExpenseCategory>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = db.ExpenseCategories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        // Tri en mémoire : le référentiel compte quelques catégories et le tri doit ignorer la casse.
        var expenseCategories = await query.ToListAsync(cancellationToken);
        return [.. expenseCategories.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public async Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        // Comparaison en mémoire pour ignorer la casse sans dépendre de la traduction SQL.
        var expenseCategories = await db.ExpenseCategories.AsNoTracking().ToListAsync(cancellationToken);
        return expenseCategories.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(ExpenseCategory expenseCategory) => db.ExpenseCategories.Add(expenseCategory);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
