namespace UsMecanic.Application.Costs.Summary;

/// <summary>Demande de synthèse.</summary>
/// <param name="From">Premier jour de la période (obligatoire).</param>
/// <param name="To">Dernier jour de la période, inclus (obligatoire, pas avant <paramref name="From"/>).</param>
/// <param name="Granularity"><c>month</c> ou <c>year</c> (insensible à la casse) ; <c>month</c> par défaut.</param>
/// <param name="VehicleId">Véhicule ; absent, la synthèse porte sur tout le parc.</param>
/// <param name="OwnerCompanyId">Société propriétaire actuelle des véhicules, quelle que soit la date de la dépense.</param>
/// <param name="LesseeCompanyId">Société locataire : dépenses datées pendant une de ses locations (hors emprunts).</param>
public sealed record CostSummaryQuery(DateOnly? From, DateOnly? To, string? Granularity, Guid? VehicleId, Guid? OwnerCompanyId = null, Guid? LesseeCompanyId = null);
