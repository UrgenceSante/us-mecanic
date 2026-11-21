using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>Port de persistance des métadonnées des documents : ne charge jamais le contenu.</summary>
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Documents d'un véhicule (rattachés au véhicule ou facturés), du plus récent au plus ancien.</summary>
    Task<IReadOnlyList<Document>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>Factures d'une intervention du véhicule, du plus récent au plus ancien.</summary>
    Task<IReadOnlyList<Document>> ListByInterventionAsync(Guid vehicleId, Guid interventionId, CancellationToken cancellationToken);

    /// <summary>Factures d'une dépense du véhicule, du plus récent au plus ancien.</summary>
    Task<IReadOnlyList<Document>> ListByExpenseAsync(Guid vehicleId, Guid expenseId, CancellationToken cancellationToken);

    /// <summary>Nombre de documents par intervention du véhicule (une seule requête groupée ; absentes = 0).</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountByInterventionAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>Nombre de documents par dépense du véhicule (une seule requête groupée ; absentes = 0).</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountByExpenseAsync(Guid vehicleId, CancellationToken cancellationToken);

    void Add(Document document);

    void Remove(Document document);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
