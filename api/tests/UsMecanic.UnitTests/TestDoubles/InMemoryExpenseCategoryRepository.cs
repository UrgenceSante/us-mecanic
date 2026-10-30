using UsMecanic.Application.Costs.ExpenseCategories;
using UsMecanic.Domain.Costs;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des catégories : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly List<ExpenseCategory> _pending = [];

    public List<ExpenseCategory> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(t => t.Id == id));

    public Task<IReadOnlyList<ExpenseCategory>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ExpenseCategory>>([.. Saved.Where(t => includeInactive || t.IsActive).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)]);

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(ExpenseCategory expenseCategory) => _pending.Add(expenseCategory);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public ExpenseCategory Seed(string name, bool active = true)
    {
        var expenseCategory = ExpenseCategory.Create(name).Value;
        if (!active)
        {
            expenseCategory.Deactivate();
        }

        Saved.Add(expenseCategory);
        return expenseCategory;
    }
}
