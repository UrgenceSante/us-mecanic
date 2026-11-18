using UsMecanic.Application.Companies;
using UsMecanic.Domain.Rentals;

namespace UsMecanic.Application.Rentals;

/// <summary>Location d'un véhicule ; <c>State</c> est calculé à la date du jour (règle R5).</summary>
public sealed record RentalDto(Guid Id, Guid VehicleId, CompanyDto Tenant, DateOnly StartDate, DateOnly? EndDate, RentalState State)
{
    public static RentalDto From(Rental rental, CompanyDto tenant, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(rental);
        ArgumentNullException.ThrowIfNull(tenant);

        return new(rental.Id, rental.VehicleId, tenant, rental.StartDate, rental.EndDate, rental.StateOn(today));
    }
}
