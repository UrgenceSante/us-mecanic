using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>Lit le contenu d'un document du véhicule (consultable même hors parc).</summary>
public sealed class GetVehicleDocumentContent(MileageVehicleGuard guard, IDocumentRepository documents, IDocumentStorage storage)
{
    public async Task<Result<DocumentContentDto>> ExecuteAsync(Guid vehicleId, Guid documentId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<DocumentContentDto>.Failure(error);
        }

        var document = await documents.GetByIdAsync(documentId, cancellationToken);
        if (document is null || document.VehicleId != vehicleId)
        {
            return Result<DocumentContentDto>.Failure(DocumentErrors.NotFound);
        }

        var content = await storage.ReadAsync(documentId, cancellationToken);
        return content is null
            ? Result<DocumentContentDto>.Failure(DocumentErrors.NotFound)
            : Result<DocumentContentDto>.Success(new(document.FileName, DocumentMimeTypes.From(document.ContentType), content));
    }
}
