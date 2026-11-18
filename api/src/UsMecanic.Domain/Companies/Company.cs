using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Companies;

/// <summary>
/// Société du groupe (ambulances, pompes funèbres, transport de produits sanguins…), propriétaire ou locataire d'un véhicule.
/// Référentiel administrable : une société n'est jamais supprimée, seulement désactivée,
/// pour rester affichée sur les véhicules et locations qui la citent.
/// </summary>
public sealed class Company
{
    public const int MaxNameLength = 100;

    private Company(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    /// <summary>Une société inactive n'est plus proposée comme propriétaire ou locataire.</summary>
    public bool IsActive { get; private set; }

    public static Result<Company> Create(string? name)
    {
        var cleaned = NormalizeName(name);
        return cleaned.IsSuccess
            ? Result<Company>.Success(new Company(Guid.CreateVersion7(), cleaned.Value))
            : Result<Company>.Failure(cleaned.Error!);
    }

    public Result<Company> Rename(string? name)
    {
        var cleaned = NormalizeName(name);
        if (cleaned.IsFailure)
        {
            return Result<Company>.Failure(cleaned.Error!);
        }

        Name = cleaned.Value;
        return Result<Company>.Success(this);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Erreur à renvoyer si cette société est choisie comme propriétaire ou locataire alors qu'elle est inactive ;
    /// <c>null</c> si elle est acceptée. Les véhicules et locations qui la citent déjà ne sont pas concernés.
    /// </summary>
    public DomainError? GetSelectionError() => IsActive ? null : CompanyErrors.Inactive;

    /// <summary>Nettoie et valide un nom (obligatoire, longueur maximale) avant création ou renommage.</summary>
    public static Result<string> NormalizeName(string? name)
    {
        var cleaned = name?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<string>.Failure(CompanyErrors.NameRequired);
        }

        return cleaned.Length > MaxNameLength
            ? Result<string>.Failure(CompanyErrors.NameTooLong)
            : Result<string>.Success(cleaned);
    }
}
