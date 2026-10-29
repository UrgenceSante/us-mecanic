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
            .Where(p => p.VehicleId == vehicleId)
            .OrderByDescending(p => p.Date)
            .ThenByDescending(p => p.RecordedAt)
            .ToListAsync(cancellationToken);

    public void Add(Problem problem) => db.Problems.Add(problem);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
