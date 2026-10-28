using UsMecanic.Application.Maintenance.Garages;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des garages : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryGarageRepository : IGarageRepository
{
    private readonly List<Garage> _pending = [];

    public List<Garage> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Garage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(t => t.Id == id));

    public Task<IReadOnlyList<Garage>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Garage>>([.. Saved.Where(t => includeInactive || t.IsActive).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)]);

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(Garage garage) => _pending.Add(garage);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public Garage Seed(string name, bool active = true)
    {
        var garage = Garage.Create(name).Value;
        if (!active)
        {
            garage.Deactivate();
        }

        Saved.Add(garage);
        return garage;
    }
}
