using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Rentals;
using UsMecanic.Domain.Rentals;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Rentals;

internal sealed class RentalRepository(UsMecanicDbContext db) : IRentalRepository
{
    public Task<Rental?> GetAsync(Guid vehicleId, Guid rentalId, CancellationToken cancellationToken) =>
        db.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId && r.VehicleId == vehicleId, cancellationToken);

    public async Task<IReadOnlyList<Rental>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.Rentals
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Rental>> ListCoveringAsync(DateOnly day, IReadOnlyCollection<Guid>? vehicleIds, CancellationToken cancellationToken)
    {
        var query = db.Rentals.AsNoTracking().Where(r => r.StartDate <= day && (r.EndDate == null || r.EndDate >= day));
        if (vehicleIds is not null)
        {
            var ids = vehicleIds.ToArray();
            query = query.Where(r => ids.Contains(r.VehicleId));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<bool> HasCurrentOrUpcomingAsync(Guid vehicleId, Guid tenantCompanyId, DateOnly today, CancellationToken cancellationToken) =>
        db.Rentals.AnyAsync(
            r => r.VehicleId == vehicleId && r.TenantCompanyId == tenantCompanyId && (r.EndDate == null || r.EndDate >= today),
            cancellationToken);

    public void Add(Rental rental) => db.Rentals.Add(rental);

    public void Remove(Rental rental) => db.Rentals.Remove(rental);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
