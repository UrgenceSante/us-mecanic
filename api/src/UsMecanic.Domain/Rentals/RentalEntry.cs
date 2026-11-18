namespace UsMecanic.Domain.Rentals;

/// <summary>Valeurs saisies pour une location, avant validation. Les dates sont des jours entiers, bornes incluses.</summary>
/// <param name="TenantCompanyId">Société locataire (obligatoire).</param>
/// <param name="StartDate">Premier jour de location (obligatoire).</param>
/// <param name="EndDate">Dernier jour de location ; absent si la location n'a pas de terme connu.</param>
public sealed record RentalEntry(Guid TenantCompanyId, DateOnly StartDate, DateOnly? EndDate);
