using UsMecanic.Application.Costs.Expenses;
using UsMecanic.Domain.Costs;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des dépenses : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryExpenseRepository : IExpenseRepository
{
    private readonly List<Expense> _pending = [];
    private readonly List<Expense> _removed = [];

    public List<Expense> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<Expense>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Expense>>(
            [.. Saved.Where(e => e.VehicleId == vehicleId).OrderByDescending(e => e.Date).ThenByDescending(e => e.RecordedAt)]);

    public Task<Expense?> GetAsync(Guid vehicleId, Guid expenseId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(e => e.Id == expenseId && e.VehicleId == vehicleId));

    public void Add(Expense expense) => _pending.Add(expense);

    public void Remove(Expense expense) => _removed.Add(expense);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        Saved.RemoveAll(_removed.Contains);
        _removed.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }
}
