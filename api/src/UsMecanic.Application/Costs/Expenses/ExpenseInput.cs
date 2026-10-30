using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>Corps d'une requête « ajouter » ou « modifier » une dépense.</summary>
/// <param name="Date">Date de la dépense (obligatoire, pas dans le futur).</param>
/// <param name="CategoryId">Catégorie existante et active (obligatoire).</param>
/// <param name="Amount">Montant en euros (obligatoire) : positif ou nul, 2 décimales au plus.</param>
/// <param name="Label">Libellé facultatif.</param>
/// <param name="Family">Famille ; seule la famille ponctuelle (valeur par défaut) est saisissable.</param>
public sealed record ExpenseInput(
    DateOnly? Date = null,
    Guid? CategoryId = null,
    decimal? Amount = null,
    string? Label = null,
    ExpenseFamily Family = ExpenseFamily.OneOff);
