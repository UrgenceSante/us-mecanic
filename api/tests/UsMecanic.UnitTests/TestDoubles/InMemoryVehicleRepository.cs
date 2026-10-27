using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire : les ajouts ne sont visibles qu'après SaveChangesAsync, comme avec EF.</summary>
internal sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly List<Vehicle> _pending = [];

    public List<Vehicle> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(v => v.Id == id));

    public Task<Vehicle?> GetByPlateAsync(LicensePlate licensePlate, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(v => v.LicensePlate == licensePlate));

    public Task<IReadOnlyList<Vehicle>> GetByPlatesAsync(IReadOnlyCollection<LicensePlate> licensePlates, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Vehicle>>([.. Saved.Where(v => licensePlates.Contains(v.LicensePlate))]);

    public Task<IReadOnlyList<Vehicle>> ListAsync(VehicleFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Vehicle>>([.. Saved.Where(v => (filter.Status is null || v.Status == filter.Status) && (filter.TypeId is null || v.TypeId == filter.TypeId))]);

    public void Add(Vehicle vehicle) => _pending.Add(vehicle);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public Vehicle Seed(string plate, VehicleDetails? details = null)
    {
        var vehicle = Vehicle.Register(LicensePlate.Create(plate).Value, details ?? EmptyDetails, FixedClock.DefaultToday, FixedClock.Default.AddDays(-30)).Value;
        Saved.Add(vehicle);
        return vehicle;
    }

    public static readonly VehicleDetails EmptyDetails = new(null, null, null, null, null);
}
