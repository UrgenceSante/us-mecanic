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

    public async Task<IReadOnlyList<Intervention>> ListWithAmountInPeriodAsync(DateOnly from, DateOnly to, Guid? vehicleId, CancellationToken cancellationToken) =>
        await db.Interventions
            .AsNoTracking()
            .Where(i => i.Amount != null && i.Date >= from && i.Date <= to && (vehicleId == null || i.VehicleId == vehicleId))
            .ToListAsync(cancellationToken);

    public Task<Intervention?> GetAsync(Guid vehicleId, Guid interventionId, CancellationToken cancellationToken) =>
        db.Interventions
            .Include(i => i.Deadlines)
            .FirstOrDefaultAsync(i => i.Id == interventionId && i.VehicleId == vehicleId, cancellationToken);

    public void Add(Intervention intervention) => db.Interventions.Add(intervention);

    public void Remove(Intervention intervention)
    {
        // La FK des liens est en Restrict : ils sont supprimés explicitement, dans le même SaveChanges (même transaction).
        db.Set<InterventionDeadline>().RemoveRange(intervention.Deadlines.ToList());
        db.Interventions.Remove(intervention);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
