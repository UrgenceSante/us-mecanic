using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Documents.Categories;
using UsMecanic.Domain.Documents;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Documents;

internal sealed class DocumentCategoryRepository(UsMecanicDbContext db) : IDocumentCategoryRepository
{
    public Task<DocumentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.DocumentCategories.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DocumentCategory>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = db.DocumentCategories.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        // Tri en mémoire : le référentiel compte quelques catégories et le tri doit ignorer la casse.
        var documentCategories = await query.ToListAsync(cancellationToken);
        return [.. documentCategories.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public async Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        // Comparaison en mémoire pour ignorer la casse sans dépendre de la traduction SQL.
        var documentCategories = await db.DocumentCategories.AsNoTracking().ToListAsync(cancellationToken);
        return documentCategories.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(DocumentCategory documentCategory) => db.DocumentCategories.Add(documentCategory);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
