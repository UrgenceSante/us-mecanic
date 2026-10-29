using UsMecanic.Application.Problems;
using UsMecanic.Domain.Problems;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des problèmes : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryProblemRepository : IProblemRepository
{
    private readonly List<Problem> _pending = [];

    public List<Problem> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<Problem>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Problem>>(
            [.. Saved.Where(p => p.VehicleId == vehicleId).OrderByDescending(p => p.Date).ThenByDescending(p => p.RecordedAt)]);

    public Task<IReadOnlyList<Problem>> ListFleetAsync(FleetProblemFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Problem>>(
            [.. Saved.Where(p => (filter.Status is { } status ? p.Status == status : p.Status != ProblemStatus.Resolved)
                && (filter.VehicleId is null || p.VehicleId == filter.VehicleId)
                && (filter.NatureId is null || p.NatureId == filter.NatureId))]);

    public Task<Problem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(p => p.Id == id));

    public Task<bool> HasOpenImmobilizingAsync(Guid vehicleId, Guid exceptProblemId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(p =>
            p.VehicleId == vehicleId && p.Id != exceptProblemId && p.Immobilized == true && p.Status != ProblemStatus.Resolved));

    public void Add(Problem problem) => _pending.Add(problem);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }
}
