using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>Port de persistance des métadonnées des documents : ne charge jamais le contenu.</summary>
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Documents d'un véhicule (rattachés au véhicule ou facturés), du plus récent au plus ancien.</summary>
    Task<IReadOnlyList<Document>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    void Add(Document document);

    void Remove(Document document);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
