using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Deadlines;

internal sealed class VehicleDeadlineRepository(UsMecanicDbContext db) : IVehicleDeadlineRepository
{
    public Task<VehicleDeadline?> GetAsync(Guid vehicleId, Guid deadlineId, CancellationToken cancellationToken) =>
        db.VehicleDeadlines.FirstOrDefaultAsync(d => d.Id == deadlineId && d.VehicleId == vehicleId, cancellationToken);

    public async Task<IReadOnlyList<VehicleDeadline>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.VehicleDeadlines
            .Where(d => d.VehicleId == vehicleId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VehicleDeadline>> ListActiveAsync(Guid? vehicleId, Guid? deadlineTypeId, CancellationToken cancellationToken)
    {
        var query = db.VehicleDeadlines.AsNoTracking().Where(d => !d.IsClosed);
        if (vehicleId is { } vehicle)
        {
            query = query.Where(d => d.VehicleId == vehicle);
        }

        if (deadlineTypeId is { } type)
        {
            query = query.Where(d => d.DeadlineTypeId == type);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<bool> ActiveExistsAsync(Guid vehicleId, Guid deadlineTypeId, CancellationToken cancellationToken) =>
        db.VehicleDeadlines.AnyAsync(d => d.VehicleId == vehicleId && d.DeadlineTypeId == deadlineTypeId && !d.IsClosed, cancellationToken);

    public void Add(VehicleDeadline deadline) => db.VehicleDeadlines.Add(deadline);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
