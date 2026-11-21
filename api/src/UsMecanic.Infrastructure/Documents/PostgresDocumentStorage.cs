using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Documents;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Documents;

/// <summary>
/// Stockage du contenu dans PostgreSQL (table <c>document_contents</c>, colonne <c>bytea</c>). Partage le contexte,
/// donc la transaction, des métadonnées. N'écrit rien dans les logs.
/// </summary>
internal sealed class PostgresDocumentStorage(UsMecanicDbContext db) : IDocumentStorage
{
    public async Task SaveAsync(Guid documentId, ReadOnlyMemory<byte> content, CancellationToken cancellationToken)
    {
        db.DocumentContents.Add(new DocumentContentRecord(documentId, content.ToArray()));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<byte[]?> ReadAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var record = await db.DocumentContents.AsNoTracking()
            .FirstOrDefaultAsync(c => c.DocumentId == documentId, cancellationToken);
        return record?.Content;
    }

    public Task DeleteAsync(Guid documentId, CancellationToken cancellationToken) =>
        db.DocumentContents.Where(c => c.DocumentId == documentId).ExecuteDeleteAsync(cancellationToken);
}
