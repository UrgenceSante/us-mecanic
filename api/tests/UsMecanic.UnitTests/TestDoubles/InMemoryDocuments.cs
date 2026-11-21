using UsMecanic.Application.Documents;
using UsMecanic.Domain.Documents;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>
/// Métadonnées et contenu des documents en mémoire. Comme avec l'adaptateur PostgreSQL, les ajouts sont validés par
/// <see cref="IDocumentStorage.SaveAsync"/> et la suppression des métadonnées supprime aussi le contenu (cascade).
/// </summary>
internal sealed class InMemoryDocuments : IDocumentRepository, IDocumentStorage
{
    private readonly List<Document> _pending = [];
    private readonly Dictionary<Guid, byte[]> _contents = [];

    public List<Document> Saved { get; } = [];

    public int DeleteContentCalls { get; private set; }

    public bool HasContent(Guid documentId) => _contents.ContainsKey(documentId);

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(d => d.Id == id));

    public Task<IReadOnlyList<Document>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Document>>([.. Saved.Where(d => d.VehicleId == vehicleId).OrderByDescending(d => d.AddedAt)]);

    public Task<IReadOnlyList<Document>> ListByInterventionAsync(Guid vehicleId, Guid interventionId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Document>>(
            [.. Saved.Where(d => d.VehicleId == vehicleId && d.InterventionId == interventionId).OrderByDescending(d => d.AddedAt)]);

    public Task<IReadOnlyList<Document>> ListByExpenseAsync(Guid vehicleId, Guid expenseId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Document>>(
            [.. Saved.Where(d => d.VehicleId == vehicleId && d.ExpenseId == expenseId).OrderByDescending(d => d.AddedAt)]);

    public int CountCalls { get; private set; }

    public Task<IReadOnlyDictionary<Guid, int>> CountByInterventionAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        CountCalls++;
        return Task.FromResult<IReadOnlyDictionary<Guid, int>>(Saved
            .Where(d => d.VehicleId == vehicleId && d.InterventionId is not null)
            .GroupBy(d => d.InterventionId!.Value)
            .ToDictionary(g => g.Key, g => g.Count()));
    }

    public Task<IReadOnlyDictionary<Guid, int>> CountByExpenseAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        CountCalls++;
        return Task.FromResult<IReadOnlyDictionary<Guid, int>>(Saved
            .Where(d => d.VehicleId == vehicleId && d.ExpenseId is not null)
            .GroupBy(d => d.ExpenseId!.Value)
            .ToDictionary(g => g.Key, g => g.Count()));
    }

    public void Add(Document document) => _pending.Add(document);

    public void Remove(Document document) => _removed.Add(document);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var removed in _removed)
        {
            Saved.Remove(removed);
            _contents.Remove(removed.Id);
        }

        _removed.Clear();
        return Task.CompletedTask;
    }

    public Task SaveAsync(Guid documentId, ReadOnlyMemory<byte> content, CancellationToken cancellationToken)
    {
        _contents[documentId] = content.ToArray();
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }

    public Task<byte[]?> ReadAsync(Guid documentId, CancellationToken cancellationToken) =>
        Task.FromResult(_contents.GetValueOrDefault(documentId));

    public Task DeleteAsync(Guid documentId, CancellationToken cancellationToken)
    {
        DeleteContentCalls++;
        _contents.Remove(documentId);
        return Task.CompletedTask;
    }

    public Document Seed(Guid vehicleId, Guid categoryId, string fileName, DateTimeOffset addedAt, byte[]? content = null)
    {
        var bytes = content ?? [.. "%PDF-1.7"u8];
        var file = new DocumentFile(fileName, DocumentContentType.Pdf, bytes.Length);
        var document = Document.CreateForVehicle(vehicleId, categoryId, null, file, addedAt).Value;
        Saved.Add(document);
        _contents[document.Id] = bytes;
        return document;
    }

    public Document SeedInvoice(Guid vehicleId, Guid interventionId, DateTimeOffset addedAt)
    {
        var file = new DocumentFile("facture.pdf", DocumentContentType.Pdf, 8);
        var document = Document.CreateInvoice(vehicleId, interventionId, null, null, file, addedAt).Value;
        Saved.Add(document);
        _contents[document.Id] = [.. "%PDF-1.7"u8];
        return document;
    }

    public Document SeedExpenseInvoice(Guid vehicleId, Guid expenseId, DateTimeOffset addedAt)
    {
        var file = new DocumentFile("facture.pdf", DocumentContentType.Pdf, 8);
        var document = Document.CreateInvoice(vehicleId, null, expenseId, null, file, addedAt).Value;
        Saved.Add(document);
        _contents[document.Id] = [.. "%PDF-1.7"u8];
        return document;
    }

    private readonly List<Document> _removed = [];
}
