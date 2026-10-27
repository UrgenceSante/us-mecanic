using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Mileage;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Mileage;

internal sealed class MileageReadingRepository(UsMecanicDbContext db) : IMileageReadingRepository
{
    public Task<MileageReading?> GetAsync(Guid vehicleId, Guid readingId, CancellationToken cancellationToken) =>
        db.MileageReadings.FirstOrDefaultAsync(r => r.Id == readingId && r.VehicleId == vehicleId, cancellationToken);

    public async Task<IReadOnlyList<MileageReading>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.MileageReadings
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.RecordedAt)
            .ToListAsync(cancellationToken);

    public void Add(MileageReading reading) => db.MileageReadings.Add(reading);

    public void Remove(MileageReading reading) => db.MileageReadings.Remove(reading);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
