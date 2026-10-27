using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Vehicles.Types;
using UsMecanic.Domain.Vehicles;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Vehicles;

internal sealed class VehicleTypeRepository(UsMecanicDbContext db) : IVehicleTypeRepository
{
    public Task<VehicleType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.VehicleTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<VehicleType>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = db.VehicleTypes.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        // Tri en mémoire : le référentiel compte quelques types et le tri doit ignorer la casse.
        var types = await query.ToListAsync(cancellationToken);
        return [.. types.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public async Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        // Comparaison en mémoire pour ignorer la casse sans dépendre de la traduction SQL.
        var types = await db.VehicleTypes.AsNoTracking().ToListAsync(cancellationToken);
        return types.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(VehicleType vehicleType) => db.VehicleTypes.Add(vehicleType);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
