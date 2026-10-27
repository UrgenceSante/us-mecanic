using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Deadlines;

internal sealed class DeadlineTypeRepository(UsMecanicDbContext db) : IDeadlineTypeRepository
{
    public Task<DeadlineType?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.DeadlineTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DeadlineType>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = db.DeadlineTypes.AsNoTracking();
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
        var types = await db.DeadlineTypes.AsNoTracking().ToListAsync(cancellationToken);
        return types.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(DeadlineType deadlineType) => db.DeadlineTypes.Add(deadlineType);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
