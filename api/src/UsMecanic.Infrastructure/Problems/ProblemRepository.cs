using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Problems;
using UsMecanic.Domain.Problems;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Problems;

internal sealed class ProblemRepository(UsMecanicDbContext db) : IProblemRepository
{
    public async Task<IReadOnlyList<Problem>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.Problems
            .AsNoTracking()
            .Include(p => p.Actions.OrderBy(a => a.RecordedAt).ThenBy(a => a.Id))
            .Where(p => p.VehicleId == vehicleId)
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Problem>> ListFleetAsync(FleetProblemFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = db.Problems.AsNoTracking().Include(p => p.Actions.OrderBy(a => a.RecordedAt).ThenBy(a => a.Id)).AsQueryable();
        query = filter.Status is { } status
            ? query.Where(p => p.Status == status)
            : query.Where(p => p.Status != ProblemStatus.Resolved);
        if (filter.VehicleId is { } vehicleId)
        {
            query = query.Where(p => p.VehicleId == vehicleId);
        }

        if (filter.NatureId is { } natureId)
        {
            query = query.Where(p => p.NatureId == natureId);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Problem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Problems.Include(p => p.Actions.OrderBy(a => a.RecordedAt).ThenBy(a => a.Id)).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> HasOpenImmobilizingAsync(Guid vehicleId, Guid exceptProblemId, CancellationToken cancellationToken) =>
        db.Problems.AnyAsync(
            p => p.VehicleId == vehicleId && p.Id != exceptProblemId && p.Immobilized == true && p.Status != ProblemStatus.Resolved,
            cancellationToken);

    public void Add(Problem problem) => db.Problems.Add(problem);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
