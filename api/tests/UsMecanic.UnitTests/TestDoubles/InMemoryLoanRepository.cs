using UsMecanic.Application.Costs.Loans;
using UsMecanic.Domain.Costs;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des emprunts : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly List<Loan> _pending = [];

    public List<Loan> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Loan?> GetAsync(Guid vehicleId, Guid loanId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(l => l.Id == loanId && l.VehicleId == vehicleId));

    public Task<IReadOnlyList<Loan>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Loan>>(
            [.. Saved.Where(l => l.VehicleId == vehicleId).OrderByDescending(l => l.StartDate).ThenByDescending(l => l.RecordedAt)]);

    public Task<IReadOnlyList<Loan>> ListStartedBeforeAsync(DateOnly to, Guid? vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Loan>>(
            [.. Saved.Where(l => l.StartDate <= to && (vehicleId is null || l.VehicleId == vehicleId))]);

    public void Add(Loan loan) => _pending.Add(loan);

    public void Remove(Loan loan) => Saved.Remove(loan);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }
}
