using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Documents;
using UsMecanic.Domain.Documents;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Documents;

/// <summary>Métadonnées des documents : le contenu est dans une autre table et n'est jamais lu ici.</summary>
internal sealed class DocumentRepository(UsMecanicDbContext db) : IDocumentRepository
{
    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        await db.Documents.AsNoTracking()
            .Where(d => d.VehicleId == vehicleId)
            .OrderByDescending(d => d.AddedAt)
            .ToListAsync(cancellationToken);

    public void Add(Document document) => db.Documents.Add(document);

    public void Remove(Document document) => db.Documents.Remove(document);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
