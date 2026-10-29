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

    public void Add(Problem problem) => _pending.Add(problem);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }
}
