using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>
/// Valeur d'un référentiel administrable des problèmes. Elle n'est jamais supprimée, seulement désactivée :
/// elle n'est alors plus proposée pour une nouvelle saisie mais reste affichée là où elle est déjà utilisée.
/// </summary>
public abstract class ProblemReference
{
    public const int MaxNameLength = 100;

    protected ProblemReference(Guid id, string name, ReferenceKind kind)
    {
        Id = id;
        Name = name;
        Kind = kind;
        IsActive = true;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    /// <summary>Une valeur inactive n'est plus acceptée pour une nouvelle saisie.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Référentiel auquel appartient la valeur (non persisté).</summary>
    public ReferenceKind Kind { get; }

    public Result<ProblemReference> Rename(string? name)
    {
        var cleaned = NormalizeName(name, Kind);
        if (cleaned.IsFailure)
        {
            return Result<ProblemReference>.Failure(cleaned.Error!);
        }

        Name = cleaned.Value;
        return Result<ProblemReference>.Success(this);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Erreur à renvoyer si cette valeur est choisie pour une nouvelle analyse ou action alors qu'elle est inactive ;
    /// <c>null</c> si elle est acceptée. Les valeurs déjà enregistrées ne sont pas concernées.
    /// </summary>
    public DomainError? GetSelectionError() => IsActive ? null : ProblemReferenceErrors.Inactive(Kind);

    /// <summary>Nettoie et valide un libellé (obligatoire, longueur maximale) avant création ou renommage.</summary>
    public static Result<string> NormalizeName(string? name, ReferenceKind kind)
    {
        var cleaned = name?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<string>.Failure(ProblemReferenceErrors.NameRequired(kind));
        }

        return cleaned.Length > MaxNameLength
            ? Result<string>.Failure(ProblemReferenceErrors.NameTooLong(kind))
            : Result<string>.Success(cleaned);
    }

    /// <summary>Crée une valeur après validation du libellé ; utilisé par les fabriques des référentiels.</summary>
    protected static Result<T> CreateValidated<T>(string? name, ReferenceKind kind, Func<string, T> factory)
        where T : ProblemReference
    {
        ArgumentNullException.ThrowIfNull(factory);

        var cleaned = NormalizeName(name, kind);
        return cleaned.IsSuccess
            ? Result<T>.Success(factory(cleaned.Value))
            : Result<T>.Failure(cleaned.Error!);
    }
}
