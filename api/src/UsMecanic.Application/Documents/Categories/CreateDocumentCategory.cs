using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents.Categories;

/// <summary>Ajoute une catégorie. Le nom est obligatoire et unique (sans tenir compte de la casse).</summary>
public sealed class CreateDocumentCategory(IDocumentCategoryRepository repository)
{
    public async Task<Result<DocumentCategoryDto>> ExecuteAsync(string? name, CancellationToken cancellationToken)
    {
        var documentCategory = DocumentCategory.Create(name);
        if (documentCategory.IsFailure)
        {
            return Result<DocumentCategoryDto>.Failure(documentCategory.Error!);
        }

        if (await repository.NameExistsAsync(documentCategory.Value.Name, null, cancellationToken))
        {
            return Result<DocumentCategoryDto>.Failure(DocumentCategoryErrors.NameAlreadyExists);
        }

        repository.Add(documentCategory.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<DocumentCategoryDto>.Success(DocumentCategoryDto.From(documentCategory.Value));
    }
}
