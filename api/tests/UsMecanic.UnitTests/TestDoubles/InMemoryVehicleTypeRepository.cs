using UsMecanic.Application.Vehicles.Types;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des types de véhicule : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryVehicleTypeRepository : IVehicleTypeRepository
{
    private readonly List<VehicleType> _pending = [];

    public List<VehicleType> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<VehicleType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(t => t.Id == id));

    public Task<IReadOnlyList<VehicleType>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<VehicleType>>([.. Saved.Where(t => includeInactive || t.IsActive).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)]);

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(VehicleType vehicleType) => _pending.Add(vehicleType);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public VehicleType Seed(string name, bool active = true)
    {
        var vehicleType = VehicleType.Create(name).Value;
        if (!active)
        {
            vehicleType.Deactivate();
        }

        Saved.Add(vehicleType);
        return vehicleType;
    }
}
