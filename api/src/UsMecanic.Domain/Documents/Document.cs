using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Documents;

/// <summary>
/// Métadonnées d'un document (le contenu est stocké à part, derrière le port de stockage). Il appartient toujours à
/// un véhicule : soit rattaché au véhicule lui-même (catégorie obligatoire), soit facture d'une intervention ou
/// d'une dépense (sans catégorie). Un document ne se modifie pas : on le supprime et on en ajoute un autre.
/// </summary>
public sealed class Document
{
    public const int MaxLabelLength = 200;

    public const int MaxFileNameLength = 255;

    /// <summary>Taille maximale d'un fichier : 10 Mo.</summary>
    public const int MaxSizeBytes = 10 * 1024 * 1024;

    // Les paramètres portent le nom des propriétés : c'est ce qui permet à EF Core de rematérialiser l'entité.
    private Document(
        Guid id,
        Guid vehicleId,
        Guid? categoryId,
        Guid? interventionId,
        Guid? expenseId,
        string? label,
        string fileName,
        DocumentContentType contentType,
        long sizeBytes,
        DateTimeOffset addedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        CategoryId = categoryId;
        InterventionId = interventionId;
        ExpenseId = expenseId;
        Label = label;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        AddedAt = addedAt;
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    /// <summary>Catégorie : renseignée pour un document du véhicule, absente pour une facture.</summary>
    public Guid? CategoryId { get; }

    /// <summary>Intervention dont ce document est la facture ; <c>null</c> sinon.</summary>
    public Guid? InterventionId { get; }

    /// <summary>Dépense dont ce document est la facture ; <c>null</c> sinon.</summary>
    public Guid? ExpenseId { get; }

    public string? Label { get; }

    /// <summary>Nom du fichier d'origine, déjà nettoyé. Donnée potentiellement personnelle : jamais dans les logs.</summary>
    public string FileName { get; }

    public DocumentContentType ContentType { get; }

    public long SizeBytes { get; }

    public DateTimeOffset AddedAt { get; }

    /// <summary>Crée un document rattaché au véhicule lui-même : la catégorie est obligatoire.</summary>
    public static Result<Document> CreateForVehicle(
        Guid vehicleId, Guid? categoryId, string? label, DocumentFile file, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (categoryId is null)
        {
            return Result<Document>.Failure(DocumentErrors.CategoryRequired);
        }

        return Build(vehicleId, categoryId, null, null, label, file, now);
    }

    /// <summary>
    /// Crée la facture d'une intervention ou d'une dépense (l'une des deux exactement), sans catégorie.
    /// L'appartenance de l'intervention ou de la dépense au véhicule est contrôlée par le cas d'usage.
    /// </summary>
    public static Result<Document> CreateInvoice(
        Guid vehicleId, Guid? interventionId, Guid? expenseId, string? label, DocumentFile file, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (interventionId is null && expenseId is null)
        {
            return Result<Document>.Failure(DocumentErrors.AttachmentRequired);
        }

        if (interventionId is not null && expenseId is not null)
        {
            return Result<Document>.Failure(DocumentErrors.AttachmentAmbiguous);
        }

        return Build(vehicleId, null, interventionId, expenseId, label, file, now);
    }

    private static Result<Document> Build(
        Guid vehicleId, Guid? categoryId, Guid? interventionId, Guid? expenseId, string? label, DocumentFile file, DateTimeOffset now)
    {
        var cleanedLabel = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        if (cleanedLabel is { Length: > MaxLabelLength })
        {
            return Result<Document>.Failure(DocumentErrors.LabelTooLong);
        }

        return Result<Document>.Success(new Document(
            Guid.CreateVersion7(now), vehicleId, categoryId, interventionId, expenseId, cleanedLabel,
            file.FileName, file.ContentType, file.SizeBytes, now));
    }
}
