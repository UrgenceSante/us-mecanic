namespace UsMecanic.Application.Documents.Categories;

/// <summary>Liste les catégories. Les catégories désactivées ne sont plus proposées, sauf demande explicite.</summary>
public sealed class ListDocumentCategories(IDocumentCategoryRepository repository)
{
    public async Task<IReadOnlyList<DocumentCategoryDto>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var documentCategories = await repository.ListAsync(includeInactive, cancellationToken);
        return [.. documentCategories.Select(DocumentCategoryDto.From)];
    }
}
