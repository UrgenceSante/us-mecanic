using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Maintenance;

internal sealed class InterventionRepository(UsMecanicDbContext db) : IInterventionRepository
{
    public async Task<IReadOnlyList<Intervention>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.Interventions
            .AsNoTracking()
            .Include(i => i.Deadlines)
            .Where(i => i.VehicleId == vehicleId)
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.RecordedAt)
            .ToListAsync(cancellationToken);

    public void Add(Intervention intervention) => db.Interventions.Add(intervention);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
