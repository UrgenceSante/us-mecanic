namespace UsMecanic.Infrastructure.Documents;

/// <summary>
/// Ligne de la table de contenu (octets en <c>bytea</c>), séparée des métadonnées pour que les listes ne chargent
/// jamais les fichiers. Interne : seul l'adaptateur de stockage y accède.
/// </summary>
internal sealed class DocumentContentRecord
{
    // Les paramètres portent le nom des propriétés : c'est ce qui permet à EF Core de rematérialiser l'entité.
    public DocumentContentRecord(Guid documentId, byte[] content)
    {
        DocumentId = documentId;
        Content = content;
    }

    public Guid DocumentId { get; }

    public byte[] Content { get; }
}
