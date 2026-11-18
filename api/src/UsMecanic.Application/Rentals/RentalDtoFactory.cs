using UsMecanic.Application.Companies;
using UsMecanic.Domain.Rentals;

namespace UsMecanic.Application.Rentals;

/// <summary>Construit les DTO de location avec la société locataire (même désactivée) et l'état à la date du jour.</summary>
public sealed class RentalDtoFactory(ICompanyRepository companies, BusinessCalendar calendar)
{
    public async Task<IReadOnlyList<RentalDto>> ToDtosAsync(IReadOnlyList<Rental> rentals, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(rentals);

        var byId = (await companies.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(c => c.Id, CompanyDto.From);
        var today = calendar.Today;
        return [.. rentals.Select(r => RentalDto.From(r, byId.GetValueOrDefault(r.TenantCompanyId) ?? new CompanyDto(r.TenantCompanyId, "Société inconnue", false), today))];
    }

    public async Task<RentalDto> ToDtoAsync(Rental rental, CancellationToken cancellationToken) =>
        (await ToDtosAsync([rental], cancellationToken))[0];
}
