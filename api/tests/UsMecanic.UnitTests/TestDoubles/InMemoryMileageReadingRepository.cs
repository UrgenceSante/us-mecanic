using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Mileage;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des relevés : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryMileageReadingRepository : IMileageReadingRepository
{
    private readonly List<MileageReading> _pending = [];

    public List<MileageReading> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<MileageReading?> GetAsync(Guid vehicleId, Guid readingId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(r => r.Id == readingId && r.VehicleId == vehicleId));

    public Task<IReadOnlyList<MileageReading>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<MileageReading>>(
            [.. Saved.Where(r => r.VehicleId == vehicleId).OrderByDescending(r => r.Date).ThenByDescending(r => r.RecordedAt)]);

    public Task<IReadOnlyList<MileageReading>> ListInPeriodAsync(DateOnly firstDay, DateOnly lastDay, Guid? vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<MileageReading>>(
            [.. Saved.Where(r => r.Date >= firstDay && r.Date <= lastDay && (vehicleId is null || r.VehicleId == vehicleId))]);

    public int CurrentByVehicleCalls { get; private set; }

    public Task<CurrentMileage?> GetCurrentAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var current = CurrentMileageRule.Select(Saved.Where(r => r.VehicleId == vehicleId));
        return Task.FromResult(current is null ? null : new CurrentMileage(current.Kilometers, current.Date));
    }

    public Task<IReadOnlyDictionary<Guid, CurrentMileage>> GetCurrentByVehicleAsync(
        IReadOnlyCollection<Guid> vehicleIds, CancellationToken cancellationToken)
    {
        CurrentByVehicleCalls++;
        var result = Saved
            .Where(r => vehicleIds.Contains(r.VehicleId))
            .GroupBy(r => r.VehicleId)
            .Select(g => (g.Key, Current: CurrentMileageRule.Select(g)!))
            .ToDictionary(x => x.Key, x => new CurrentMileage(x.Current.Kilometers, x.Current.Date));
        return Task.FromResult<IReadOnlyDictionary<Guid, CurrentMileage>>(result);
    }

    public void Add(MileageReading reading) => _pending.Add(reading);

    public void Remove(MileageReading reading) => Saved.Remove(reading);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public MileageReading Seed(Guid vehicleId, DateOnly date, int kilometers, string? reason = null)
    {
        var reading = MileageReading.Create(vehicleId, new MileageReadingEntry(date, kilometers, reason, ConfirmHighMileage: true), [], FixedClock.DefaultToday, FixedClock.Default).Value;
        Saved.Add(reading);
        return reading;
    }
}
