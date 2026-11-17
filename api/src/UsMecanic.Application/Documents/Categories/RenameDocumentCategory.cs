using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents.Categories;

/// <summary>Renomme une catégorie. Le nouveau nom doit rester unique.</summary>
public sealed class RenameDocumentCategory(IDocumentCategoryRepository repository)
{
    public async Task<Result<DocumentCategoryDto>> ExecuteAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        var normalized = DocumentCategory.NormalizeName(name);
        if (normalized.IsFailure)
        {
            return Result<DocumentCategoryDto>.Failure(normalized.Error!);
        }

        var documentCategory = await repository.GetByIdAsync(id, cancellationToken);
        if (documentCategory is null)
        {
            return Result<DocumentCategoryDto>.Failure(DocumentCategoryErrors.NotFound);
        }

        if (await repository.NameExistsAsync(normalized.Value, id, cancellationToken))
        {
            return Result<DocumentCategoryDto>.Failure(DocumentCategoryErrors.NameAlreadyExists);
        }

        documentCategory.Rename(normalized.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<DocumentCategoryDto>.Success(DocumentCategoryDto.From(documentCategory));
    }
}
