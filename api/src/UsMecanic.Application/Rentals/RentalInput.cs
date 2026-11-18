using UsMecanic.Domain.Rentals;

namespace UsMecanic.Application.Rentals;

/// <summary>Corps d'une requête de création ou de modification d'une location (jours entiers, bornes incluses).</summary>
/// <param name="TenantCompanyId">Société locataire, parmi les sociétés actives.</param>
/// <param name="StartDate">Premier jour de location.</param>
/// <param name="EndDate">Dernier jour de location ; absent si la location n'a pas de terme connu.</param>
public sealed record RentalInput(Guid? TenantCompanyId, DateOnly StartDate, DateOnly? EndDate)
{
    public RentalEntry ToEntry() => new(TenantCompanyId ?? Guid.Empty, StartDate, EndDate);
}
