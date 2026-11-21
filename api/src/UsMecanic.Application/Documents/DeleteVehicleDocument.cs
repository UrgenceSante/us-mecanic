using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Supprime définitivement un document (métadonnées et contenu), y compris sur un véhicule sorti du parc.
/// Avec PostgreSQL, supprimer les métadonnées supprime le contenu par cascade : pas d'appel à <c>DeleteAsync</c>.
/// </summary>
public sealed class DeleteVehicleDocument(MileageVehicleGuard guard, IDocumentRepository documents)
{
    public async Task<Result<Guid>> ExecuteAsync(Guid vehicleId, Guid documentId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<Guid>.Failure(error);
        }

        var document = await documents.GetByIdAsync(documentId, cancellationToken);
        if (document is null || document.VehicleId != vehicleId)
        {
            return Result<Guid>.Failure(DocumentErrors.NotFound);
        }

        documents.Remove(document);
        await documents.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(documentId);
    }
}
