using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des interventions : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryInterventionRepository : IInterventionRepository
{
    private readonly List<Intervention> _pending = [];

    public List<Intervention> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public int ListCalls { get; private set; }

    public Task<IReadOnlyList<Intervention>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        ListCalls++;
        return Task.FromResult<IReadOnlyList<Intervention>>(
            [.. Saved.Where(i => i.VehicleId == vehicleId).OrderByDescending(i => i.Date).ThenByDescending(i => i.RecordedAt)]);
    }

    public void Add(Intervention intervention) => _pending.Add(intervention);

    /// <summary>Autres dépôts enregistrés dans la même unité de travail (un seul SaveChanges en production).</summary>
    public List<Func<Task>> SharedSaves { get; } = [];

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        foreach (var save in SharedSaves)
        {
            await save();
        }
    }
}
