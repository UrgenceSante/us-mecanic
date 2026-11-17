using UsMecanic.Application.Documents.Categories;
using UsMecanic.Domain.Documents;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des catégories : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryDocumentCategoryRepository : IDocumentCategoryRepository
{
    private readonly List<DocumentCategory> _pending = [];

    public List<DocumentCategory> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<DocumentCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(t => t.Id == id));

    public Task<IReadOnlyList<DocumentCategory>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DocumentCategory>>([.. Saved.Where(t => includeInactive || t.IsActive).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)]);

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(DocumentCategory documentCategory) => _pending.Add(documentCategory);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public DocumentCategory Seed(string name, bool active = true)
    {
        var documentCategory = DocumentCategory.Create(name).Value;
        if (!active)
        {
            documentCategory.Deactivate();
        }

        Saved.Add(documentCategory);
        return documentCategory;
    }
}
