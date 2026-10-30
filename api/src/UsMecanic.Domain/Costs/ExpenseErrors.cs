using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Costs;

public static class ExpenseErrors
{
    /// <summary>Dépense inexistante ou appartenant à un autre véhicule.</summary>
    public static readonly DomainError NotFound =
        DomainError.NotFound("expense.not_found", "Dépense introuvable.");

    public static readonly DomainError DateRequired =
        new("expense.date_required", "La date de la dépense est obligatoire.");

    public static readonly DomainError DateInFuture =
        new("expense.date_in_future", "La date de la dépense ne peut pas être dans le futur.");

    public static readonly DomainError AmountRequired =
        new("expense.amount_required", "Le montant de la dépense est obligatoire.");

    public static readonly DomainError AmountNegative =
        new("expense.amount_negative", "Le montant ne peut pas être négatif.");

    public static readonly DomainError AmountTooPrecise =
        new("expense.amount_too_precise", "Le montant est limité à 2 décimales.");

    public static readonly DomainError AmountTooLarge =
        new("expense.amount_too_large", "Le montant dépasse la valeur maximale autorisée.");

    public static readonly DomainError LabelTooLong =
        new("expense.label_too_long", $"Le libellé est limité à {Expense.MaxLabelLength} caractères.");

    public static readonly DomainError CategoryRequired =
        new("expense.category_required", "La catégorie d'une dépense ponctuelle est obligatoire.");

    public static readonly DomainError FamilyNotManual =
        new("expense.family_not_manual", "Les dépenses d'utilisation courante (carburant, péage) ne se saisissent pas à la main.");

    public static readonly DomainError NotManual =
        DomainError.Conflict("expense.not_manual", "Une dépense issue d'un connecteur ne peut être ni modifiée ni supprimée.");
}
