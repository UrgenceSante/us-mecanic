using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Vehicles;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Vehicles;

internal sealed class VehicleRepository(UsMecanicDbContext db) : IVehicleRepository
{
    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<Vehicle?> GetByPlateAsync(LicensePlate licensePlate, CancellationToken cancellationToken) =>
        db.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetByPlatesAsync(IReadOnlyCollection<LicensePlate> licensePlates, CancellationToken cancellationToken) =>
        await db.Vehicles.Where(v => licensePlates.Contains(v.LicensePlate)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> ListAsync(VehicleFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = db.Vehicles.AsNoTracking();
        if (filter.Status is { } status)
        {
            query = query.Where(v => v.Status == status);
        }

        if (filter.TypeId is { } typeId)
        {
            query = query.Where(v => v.TypeId == typeId);
        }

        // La recherche texte est faite en mémoire : le parc compte quelques centaines de véhicules
        // et l'immatriculation est un value object (non traduisible en LIKE par EF).
        var vehicles = await query.ToListAsync(cancellationToken);
        return [.. vehicles.Where(v => Matches(v, filter.Search)).OrderBy(v => v.LicensePlate.Value, StringComparer.Ordinal)];
    }

    public void Add(Vehicle vehicle) => db.Vehicles.Add(vehicle);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    private static bool Matches(Vehicle vehicle, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var term = search.Trim();
        var compactTerm = term.Replace("-", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal);
        return vehicle.LicensePlate.Value.Replace("-", string.Empty, StringComparison.Ordinal).Contains(compactTerm, StringComparison.OrdinalIgnoreCase)
            || new[] { vehicle.Brand, vehicle.Model, vehicle.InternalNumber }
                .Any(text => text?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
    }
}
