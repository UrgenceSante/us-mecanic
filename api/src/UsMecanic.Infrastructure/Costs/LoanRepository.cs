using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Costs.Loans;
using UsMecanic.Domain.Costs;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Costs;

internal sealed class LoanRepository(UsMecanicDbContext db) : ILoanRepository
{
    public Task<Loan?> GetAsync(Guid vehicleId, Guid loanId, CancellationToken cancellationToken) =>
        db.Loans.FirstOrDefaultAsync(l => l.Id == loanId && l.VehicleId == vehicleId, cancellationToken);

    public async Task<IReadOnlyList<Loan>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.Loans
            .Where(l => l.VehicleId == vehicleId)
            .OrderByDescending(l => l.StartDate)
            .ThenByDescending(l => l.RecordedAt)
            .ToListAsync(cancellationToken);

    public void Add(Loan loan) => db.Loans.Add(loan);

    public void Remove(Loan loan) => db.Loans.Remove(loan);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
