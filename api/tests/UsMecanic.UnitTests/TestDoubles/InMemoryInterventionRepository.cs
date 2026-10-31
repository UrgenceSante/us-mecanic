using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des interventions : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryInterventionRepository : IInterventionRepository
{
    private readonly List<Intervention> _pending = [];
    private readonly List<Intervention> _removed = [];

    public List<Intervention> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public int ListCalls { get; private set; }

    public Task<IReadOnlyList<Intervention>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        ListCalls++;
        return Task.FromResult<IReadOnlyList<Intervention>>(
            [.. Saved.Where(i => i.VehicleId == vehicleId).OrderByDescending(i => i.Date).ThenByDescending(i => i.RecordedAt)]);
    }

    public Task<IReadOnlyList<Intervention>> ListWithAmountInPeriodAsync(DateOnly from, DateOnly to, Guid? vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Intervention>>(
            [.. Saved.Where(i => i.Amount != null && i.Date >= from && i.Date <= to && (vehicleId is null || i.VehicleId == vehicleId))]);

    public Task<Intervention?> GetAsync(Guid vehicleId, Guid interventionId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(i => i.Id == interventionId && i.VehicleId == vehicleId));

    public void Add(Intervention intervention) => _pending.Add(intervention);

    public void Remove(Intervention intervention) => _removed.Add(intervention);

    /// <summary>Autres dépôts enregistrés dans la même unité de travail (un seul SaveChanges en production).</summary>
    public List<Func<Task>> SharedSaves { get; } = [];

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        Saved.RemoveAll(_removed.Contains);
        _removed.Clear();
        SaveCount++;
        foreach (var save in SharedSaves)
        {
            await save();
        }
    }
}
