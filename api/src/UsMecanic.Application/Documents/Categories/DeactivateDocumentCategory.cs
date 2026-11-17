using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents.Categories;

/// <summary>
/// Désactive une catégorie : elle n'est plus proposée mais reste affichée sur les documents déjà classés.
/// Il n'existe volontairement aucune suppression de catégorie.
/// </summary>
public sealed class DeactivateDocumentCategory(IDocumentCategoryRepository repository)
{
    public async Task<Result<DocumentCategoryDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var documentCategory = await repository.GetByIdAsync(id, cancellationToken);
        if (documentCategory is null)
        {
            return Result<DocumentCategoryDto>.Failure(DocumentCategoryErrors.NotFound);
        }

        documentCategory.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return Result<DocumentCategoryDto>.Success(DocumentCategoryDto.From(documentCategory));
    }
}
