using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Costs;

public static class ExpenseCategoryErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("expense_category.not_found", "Catégorie de dépense introuvable.");

    public static readonly DomainError NameRequired =
        new("expense_category.name_required", "Le nom de la catégorie est obligatoire.");

    public static readonly DomainError NameTooLong =
        new("expense_category.name_too_long", $"Le nom de la catégorie est limité à {ExpenseCategory.MaxNameLength} caractères.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict("expense_category.name_already_exists", "Une catégorie porte déjà ce nom.");

    public static readonly DomainError Unknown =
        new("expense_category.unknown", "Catégorie de dépense inconnue.");

    public static readonly DomainError Inactive =
        new("expense_category.inactive", "Cette catégorie est désactivée : elle ne peut plus être choisie pour une nouvelle dépense.");
}
