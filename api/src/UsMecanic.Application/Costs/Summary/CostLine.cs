namespace UsMecanic.Application.Costs.Summary;

/// <summary>Un coût élémentaire à agréger : sa date (premier jour du mois pour une mensualité), sa famille, sa catégorie et son montant ; <c>CategoryId</c> : catégorie administrée (<c>null</c> pour les libellés fixes).</summary>
public sealed record CostLine(DateOnly Date, CostFamily Family, string Category, decimal Amount, Guid? CategoryId = null);
