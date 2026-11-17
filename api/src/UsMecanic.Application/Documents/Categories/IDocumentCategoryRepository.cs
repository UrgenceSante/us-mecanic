using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents.Categories;

/// <summary>Port de persistance des catégories.</summary>
public interface IDocumentCategoryRepository
{
    Task<DocumentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Catégories triées par nom ; les catégories désactivées ne sont incluses que sur demande.</summary>
    Task<IReadOnlyList<DocumentCategory>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    /// <summary>Indique si une autre catégorie (hors <paramref name="exceptId"/>) porte ce nom, sans tenir compte de la casse.</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    void Add(DocumentCategory documentCategory);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
