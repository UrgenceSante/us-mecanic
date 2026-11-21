using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Liste les documents d'un véhicule (consultable même hors parc), métadonnées seulement, du plus récent au plus
/// ancien, avec un filtre facultatif par catégorie.
/// Point d'extension (factures des interventions et des dépenses, issue #195) : le dépôt renvoie déjà les factures ;
/// elles sont exclues ici par <c>IsVehicleDocument</c> tant que leur affichage (rattachement, filtre « Factures »)
/// n'est pas livré. #195 retire ce filtre et enrichit <see cref="DocumentDto"/>.
/// </summary>
public sealed class ListVehicleDocuments(MileageVehicleGuard guard, IDocumentRepository documents)
{
    public async Task<Result<IReadOnlyList<DocumentDto>>> ExecuteAsync(Guid vehicleId, Guid? categoryId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(error);
        }

        var list = await documents.ListByVehicleAsync(vehicleId, cancellationToken);
        return Result<IReadOnlyList<DocumentDto>>.Success([.. list
            .Where(d => IsVehicleDocument(d) && (categoryId is null || d.CategoryId == categoryId))
            .OrderByDescending(d => d.AddedAt)
            .Select(DocumentDto.From)]);
    }

    private static bool IsVehicleDocument(Document document) =>
        document.InterventionId is null && document.ExpenseId is null;
}
