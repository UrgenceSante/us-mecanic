using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Costs;

/// <summary>
/// Catégorie d'une dépense ponctuelle (hors carburant et péage, catégories fixes de la phase 8).
/// Référentiel administrable : une catégorie n'est jamais supprimée, seulement désactivée,
/// pour rester affichée sur les dépenses qui l'ont.
/// </summary>
public sealed class ExpenseCategory
{
    public const int MaxNameLength = 100;

    private ExpenseCategory(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    /// <summary>Une catégorie inactive n'est plus proposée pour de nouvelles dépenses.</summary>
    public bool IsActive { get; private set; }

    public static Result<ExpenseCategory> Create(string? name)
    {
        var cleaned = NormalizeName(name);
        return cleaned.IsSuccess
            ? Result<ExpenseCategory>.Success(new ExpenseCategory(Guid.CreateVersion7(), cleaned.Value))
            : Result<ExpenseCategory>.Failure(cleaned.Error!);
    }

    public Result<ExpenseCategory> Rename(string? name)
    {
        var cleaned = NormalizeName(name);
        if (cleaned.IsFailure)
        {
            return Result<ExpenseCategory>.Failure(cleaned.Error!);
        }

        Name = cleaned.Value;
        return Result<ExpenseCategory>.Success(this);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Erreur à renvoyer si cette catégorie est choisie pour une nouvelle dépense alors qu'elle est inactive ;
    /// <c>null</c> si elle est acceptée. Les dépenses déjà enregistrées ne sont pas concernées.
    /// </summary>
    public DomainError? GetSelectionError() => IsActive ? null : ExpenseCategoryErrors.Inactive;

    /// <summary>Nettoie et valide un nom (obligatoire, longueur maximale) avant création ou renommage.</summary>
    public static Result<string> NormalizeName(string? name)
    {
        var cleaned = name?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<string>.Failure(ExpenseCategoryErrors.NameRequired);
        }

        return cleaned.Length > MaxNameLength
            ? Result<string>.Failure(ExpenseCategoryErrors.NameTooLong)
            : Result<string>.Success(cleaned);
    }
}
