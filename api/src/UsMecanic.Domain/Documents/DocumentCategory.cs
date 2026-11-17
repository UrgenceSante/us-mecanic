using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Documents;

/// <summary>
/// Catégorie de classement d'un document du véhicule (carte grise, assurance, PV de contrôle technique…).
/// Référentiel administrable : une catégorie n'est jamais supprimée, seulement désactivée,
/// pour rester affichée sur les documents déjà classés.
/// </summary>
public sealed class DocumentCategory
{
    public const int MaxNameLength = 100;

    private DocumentCategory(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    /// <summary>Une catégorie inactive n'est plus proposée pour de nouveaux documents.</summary>
    public bool IsActive { get; private set; }

    public static Result<DocumentCategory> Create(string? name)
    {
        var cleaned = NormalizeName(name);
        return cleaned.IsSuccess
            ? Result<DocumentCategory>.Success(new DocumentCategory(Guid.CreateVersion7(), cleaned.Value))
            : Result<DocumentCategory>.Failure(cleaned.Error!);
    }

    public Result<DocumentCategory> Rename(string? name)
    {
        var cleaned = NormalizeName(name);
        if (cleaned.IsFailure)
        {
            return Result<DocumentCategory>.Failure(cleaned.Error!);
        }

        Name = cleaned.Value;
        return Result<DocumentCategory>.Success(this);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Erreur à renvoyer si cette catégorie est choisie pour une nouveau document alors qu'elle est inactive ;
    /// <c>null</c> si elle est acceptée. Les documents déjà classés ne sont pas concernées.
    /// </summary>
    public DomainError? GetSelectionError() => IsActive ? null : DocumentCategoryErrors.Inactive;

    /// <summary>Nettoie et valide un nom (obligatoire, longueur maximale) avant création ou renommage.</summary>
    public static Result<string> NormalizeName(string? name)
    {
        var cleaned = name?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<string>.Failure(DocumentCategoryErrors.NameRequired);
        }

        return cleaned.Length > MaxNameLength
            ? Result<string>.Failure(DocumentCategoryErrors.NameTooLong)
            : Result<string>.Success(cleaned);
    }
}
