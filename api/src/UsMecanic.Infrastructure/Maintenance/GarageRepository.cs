using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Maintenance.Garages;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Maintenance;

internal sealed class GarageRepository(UsMecanicDbContext db) : IGarageRepository
{
    public Task<Garage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Garages.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Garage>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = db.Garages.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        // Tri en mémoire : le référentiel compte quelques garages et le tri doit ignorer la casse.
        var garages = await query.ToListAsync(cancellationToken);
        return [.. garages.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public async Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        // Comparaison en mémoire pour ignorer la casse sans dépendre de la traduction SQL.
        var garages = await db.Garages.AsNoTracking().ToListAsync(cancellationToken);
        return garages.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Garage garage) => db.Garages.Add(garage);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
