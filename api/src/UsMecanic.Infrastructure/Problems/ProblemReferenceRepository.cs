using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Problems.References;
using UsMecanic.Domain.Problems;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Problems;

public sealed class ProblemReferenceRepository<T>(UsMecanicDbContext db) : IProblemReferenceRepository<T>
    where T : ProblemReference, IProblemReference<T>
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Set<T>().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = db.Set<T>().AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        // Tri en mémoire : un référentiel compte quelques dizaines de valeurs et le tri doit ignorer la casse.
        var items = await query.ToListAsync(cancellationToken);
        return [.. items.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public async Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        // Comparaison en mémoire pour ignorer la casse sans dépendre de la traduction SQL.
        var items = await db.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
        return items.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(T item) => db.Set<T>().Add(item);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
