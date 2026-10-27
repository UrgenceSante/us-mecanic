using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des échéances de véhicule : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryVehicleDeadlineRepository : IVehicleDeadlineRepository
{
    private readonly List<VehicleDeadline> _pending = [];

    public List<VehicleDeadline> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<VehicleDeadline?> GetAsync(Guid vehicleId, Guid deadlineId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(d => d.Id == deadlineId && d.VehicleId == vehicleId));

    public Task<IReadOnlyList<VehicleDeadline>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<VehicleDeadline>>([.. Saved.Where(d => d.VehicleId == vehicleId)]);

    public Task<IReadOnlyList<VehicleDeadline>> ListActiveAsync(Guid? vehicleId, Guid? deadlineTypeId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<VehicleDeadline>>(
            [.. Saved.Where(d => !d.IsClosed && (vehicleId is null || d.VehicleId == vehicleId) && (deadlineTypeId is null || d.DeadlineTypeId == deadlineTypeId))]);

    public Task<bool> ActiveExistsAsync(Guid vehicleId, Guid deadlineTypeId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(d => d.VehicleId == vehicleId && d.DeadlineTypeId == deadlineTypeId && !d.IsClosed));

    public void Add(VehicleDeadline deadline) => _pending.Add(deadline);

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
