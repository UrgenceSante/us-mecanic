using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Métadonnées d'un document pour les listes : jamais de contenu. <see cref="CategoryId"/> est renseignée pour un
/// document du véhicule ; pour une facture, <see cref="InterventionId"/> ou <see cref="ExpenseId"/> l'est à la place.
/// </summary>
public sealed record DocumentDto(
    Guid Id,
    Guid VehicleId,
    Guid? CategoryId,
    Guid? InterventionId,
    Guid? ExpenseId,
    string? Label,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset AddedAt)
{
    public static DocumentDto From(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new(
            document.Id, document.VehicleId, document.CategoryId, document.InterventionId, document.ExpenseId,
            document.Label, document.FileName, DocumentMimeTypes.From(document.ContentType), document.SizeBytes, document.AddedAt);
    }
}
