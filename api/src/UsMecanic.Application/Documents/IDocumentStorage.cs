namespace UsMecanic.Application.Documents;

/// <summary>
/// Port de stockage du contenu des documents, identifié par l'identifiant du document (ADR 0002).
/// Les cas d'usage ne savent pas où le contenu est conservé. Le contenu n'est jamais écrit dans les logs.
/// </summary>
public interface IDocumentStorage
{
    /// <summary>
    /// Enregistre le contenu d'un document. Avec l'adaptateur PostgreSQL, l'enregistrement partage l'unité de travail
    /// des métadonnées : les métadonnées ajoutées au dépôt avant l'appel sont validées dans la même transaction.
    /// </summary>
    Task SaveAsync(Guid documentId, ReadOnlyMemory<byte> content, CancellationToken cancellationToken);

    /// <summary>Lit le contenu entier d'un document ; <c>null</c> s'il n'existe pas.</summary>
    Task<byte[]?> ReadAsync(Guid documentId, CancellationToken cancellationToken);

    /// <summary>Supprime définitivement le contenu d'un document (sans effet s'il n'existe pas).</summary>
    Task DeleteAsync(Guid documentId, CancellationToken cancellationToken);
}
