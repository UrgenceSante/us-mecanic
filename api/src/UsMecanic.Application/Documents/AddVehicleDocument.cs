using UsMecanic.Application.Documents.Categories;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Joint un fichier à un véhicule (PDF, JPEG ou PNG, 10 Mo au plus, catégorie active obligatoire). Un véhicule sorti
/// du parc accepte l'ajout (certificat de cession, facture tardive). Ni le nom ni le contenu ne sont journalisés.
/// </summary>
public sealed class AddVehicleDocument(
    MileageVehicleGuard guard,
    IDocumentCategoryRepository categories,
    IDocumentRepository documents,
    IDocumentStorage storage,
    TimeProvider clock)
{
    public async Task<Result<DocumentDto>> ExecuteAsync(
        Guid vehicleId,
        Guid? categoryId,
        string? label,
        string? fileName,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<DocumentDto>.Failure(error);
        }

        var category = await CheckCategoryAsync(categoryId, cancellationToken);
        if (category.IsFailure)
        {
            return Result<DocumentDto>.Failure(category.Error!);
        }

        var file = DocumentFileValidator.Validate(content.Span, fileName);
        if (file.IsFailure)
        {
            return Result<DocumentDto>.Failure(file.Error!);
        }

        var document = Document.CreateForVehicle(vehicleId, categoryId, label, file.Value, clock.GetUtcNow());
        if (document.IsFailure)
        {
            return Result<DocumentDto>.Failure(document.Error!);
        }

        // Les métadonnées et le contenu sont validés ensemble par SaveAsync (même unité de travail).
        documents.Add(document.Value);
        await storage.SaveAsync(document.Value.Id, content, cancellationToken);
        return Result<DocumentDto>.Success(DocumentDto.From(document.Value));
    }

    private async Task<Result<Guid>> CheckCategoryAsync(Guid? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is null)
        {
            return Result<Guid>.Failure(DocumentErrors.CategoryRequired);
        }

        var category = await categories.GetByIdAsync(categoryId.Value, cancellationToken);
        if (category is null)
        {
            return Result<Guid>.Failure(DocumentCategoryErrors.Unknown);
        }

        var selectionError = category.GetSelectionError();
        return selectionError is null ? Result<Guid>.Success(category.Id) : Result<Guid>.Failure(selectionError);
    }
}
