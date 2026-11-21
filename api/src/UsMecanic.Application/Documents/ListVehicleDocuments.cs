using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Liste les documents d'un véhicule (consultable même hors parc), métadonnées seulement, du plus récent au plus
/// ancien : documents du véhicule et factures (avec leur rattachement). Filtres facultatifs : par catégorie, ou
/// factures seulement.
/// </summary>
public sealed class ListVehicleDocuments(MileageVehicleGuard guard, IDocumentRepository documents, InvoiceAttachmentLabeler labeler)
{
    public async Task<Result<IReadOnlyList<DocumentDto>>> ExecuteAsync(
        Guid vehicleId, Guid? categoryId, bool invoicesOnly, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(error);
        }

        var list = await documents.ListByVehicleAsync(vehicleId, cancellationToken);
        var filtered = list
            .Where(d => (categoryId is null || d.CategoryId == categoryId) && (!invoicesOnly || d.CategoryId is null))
            .OrderByDescending(d => d.AddedAt)
            .ToList();
        return Result<IReadOnlyList<DocumentDto>>.Success(await labeler.LabelAsync(vehicleId, filtered, cancellationToken));
    }
}
