using UsMecanic.Application.Problems.References;
using UsMecanic.Domain.Problems;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire d'un référentiel des problèmes : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryProblemReferenceRepository<T> : IProblemReferenceRepository<T>
    where T : ProblemReference, IProblemReference<T>
{
    private readonly List<T> _pending = [];

    public List<T> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(t => t.Id == id));

    public Task<IReadOnlyList<T>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<T>>([.. Saved.Where(t => includeInactive || t.IsActive).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)]);

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(T item) => _pending.Add(item);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public T Seed(string name, bool active = true)
    {
        var item = T.Create(name).Value;
        if (!active)
        {
            item.Deactivate();
        }

        Saved.Add(item);
        return item;
    }
}
