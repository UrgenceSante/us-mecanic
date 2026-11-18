using UsMecanic.Application.Rentals;
using UsMecanic.Domain.Rentals;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des locations : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryRentalRepository : IRentalRepository
{
    private readonly List<Rental> _pending = [];

    public List<Rental> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Rental?> GetAsync(Guid vehicleId, Guid rentalId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(r => r.Id == rentalId && r.VehicleId == vehicleId));

    public Task<IReadOnlyList<Rental>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Rental>>([.. Saved.Where(r => r.VehicleId == vehicleId).OrderByDescending(r => r.StartDate)]);

    public Task<IReadOnlyList<Rental>> ListCoveringAsync(DateOnly day, IReadOnlyCollection<Guid>? vehicleIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Rental>>([.. Saved.Where(r => r.Covers(day) && (vehicleIds is null || vehicleIds.Contains(r.VehicleId)))]);

    public Task<bool> HasCurrentOrUpcomingAsync(Guid vehicleId, Guid tenantCompanyId, DateOnly today, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(r => r.VehicleId == vehicleId && r.TenantCompanyId == tenantCompanyId && r.IsCurrentOrUpcoming(today)));

    public void Add(Rental rental) => _pending.Add(rental);

    public void Remove(Rental rental) => Saved.Remove(rental);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public Rental Seed(Guid vehicleId, Guid tenantCompanyId, DateOnly startDate, DateOnly? endDate)
    {
        var rental = Rental.Create(vehicleId, new RentalEntry(tenantCompanyId, startDate, endDate)).Value;
        Saved.Add(rental);
        return rental;
    }
}
