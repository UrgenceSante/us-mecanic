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

    public async Task<IReadOnlyList<MileageReading>> ListInPeriodAsync(
        DateOnly firstDay, DateOnly lastDay, Guid? vehicleId, CancellationToken cancellationToken) =>
        await db.MileageReadings
            .AsNoTracking()
            .Where(r => r.Date >= firstDay && r.Date <= lastDay && (vehicleId == null || r.VehicleId == vehicleId))
            .ToListAsync(cancellationToken);

    // Règle : voir CurrentMileageRule (date la plus récente, puis km le plus élevé).
    public async Task<CurrentMileage?> GetCurrentAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        return await db.MileageReadings
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Kilometers)
            .Select(r => new CurrentMileage(r.Kilometers, r.Date))
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Une seule requête pour tous les véhicules (NOT EXISTS), sans N+1.
    public async Task<IReadOnlyDictionary<Guid, CurrentMileage>> GetCurrentByVehicleAsync(
        IReadOnlyCollection<Guid> vehicleIds, CancellationToken cancellationToken)
    {
        var latest = await db.MileageReadings
            .Where(r => vehicleIds.Contains(r.VehicleId))
            .Where(r => !db.MileageReadings.Any(o =>
                o.VehicleId == r.VehicleId && (o.Date > r.Date || (o.Date == r.Date && o.Kilometers > r.Kilometers))))
            .Select(r => new { r.VehicleId, r.Kilometers, r.Date })
            .ToListAsync(cancellationToken);

        // Deux relevés identiques (même date et même km) peuvent subsister : on n'en garde qu'un par véhicule.
        return latest
            .GroupBy(r => r.VehicleId)
            .ToDictionary(g => g.Key, g => new CurrentMileage(g.First().Kilometers, g.First().Date));
    }

    public void Add(MileageReading reading) => db.MileageReadings.Add(reading);

    public void Remove(MileageReading reading) => db.MileageReadings.Remove(reading);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
