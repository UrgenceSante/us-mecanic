using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des types d'échéance : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryDeadlineTypeRepository : IDeadlineTypeRepository
{
    private readonly List<DeadlineType> _pending = [];

    public List<DeadlineType> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<DeadlineType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(t => t.Id == id));

    public Task<IReadOnlyList<DeadlineType>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DeadlineType>>([.. Saved.Where(t => includeInactive || t.IsActive).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)]);

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(DeadlineType deadlineType) => _pending.Add(deadlineType);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public DeadlineType Seed(string name, bool active = true)
    {
        var type = DeadlineType.Create(name, null).Value;
        if (!active)
        {
            type.Deactivate();
        }

        Saved.Add(type);
        return type;
    }
}
