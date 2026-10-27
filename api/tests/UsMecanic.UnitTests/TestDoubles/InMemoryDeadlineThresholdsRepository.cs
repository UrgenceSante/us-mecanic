using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.TestDoubles;

internal sealed class InMemoryDeadlineThresholdsRepository : IDeadlineThresholdsRepository
{
    public DeadlineThresholds Thresholds { get; } = DeadlineThresholds.CreateDefault();

    public int SaveCount { get; private set; }

    public Task<DeadlineThresholds> GetAsync(CancellationToken cancellationToken) => Task.FromResult(Thresholds);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
