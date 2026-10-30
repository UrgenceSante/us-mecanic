using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Costs.Expenses;
using UsMecanic.Domain.Costs;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Costs;

internal sealed class ExpenseRepository(UsMecanicDbContext db) : IExpenseRepository
{
    public async Task<IReadOnlyList<Expense>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.Expenses
            .AsNoTracking()
            .Where(e => e.VehicleId == vehicleId)
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.RecordedAt)
            .ToListAsync(cancellationToken);

    public Task<Expense?> GetAsync(Guid vehicleId, Guid expenseId, CancellationToken cancellationToken) =>
        db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && e.VehicleId == vehicleId, cancellationToken);

    public void Add(Expense expense) => db.Expenses.Add(expense);

    public void Remove(Expense expense) => db.Expenses.Remove(expense);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
