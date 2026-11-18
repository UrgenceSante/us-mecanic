using UsMecanic.Application.Companies;
using UsMecanic.Application.Rentals;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Companies;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Relie les véhicules à leur société propriétaire : contrôle qu'une société peut être attribuée et fournit
/// propriétaire et utilisateur actuel aux DTO (une société désactivée reste affichée sur les véhicules qui la citent).
/// L'utilisateur actuel suit la règle R4 à la date du jour, avec une seule requête de locations pour une liste.
/// </summary>
public sealed class VehicleOwnerLookup(ICompanyRepository companies, IRentalRepository rentals, BusinessCalendar calendar)
{
    /// <summary>
    /// Une société doit exister et être active pour être choisie ; un véhicule peut en revanche conserver
    /// son propriétaire actuel même désactivé. Une société qui loue le véhicule (location en cours ou à venir)
    /// ne peut pas en devenir propriétaire. Renvoie <c>null</c> si l'attribution est permise.
    /// </summary>
    /// <param name="ownerCompanyId">Propriétaire demandé.</param>
    /// <param name="currentOwnerCompanyId">Propriétaire actuel du véhicule.</param>
    /// <param name="vehicleId">Véhicule modifié ; <c>null</c> à la création (aucune location possible).</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    public async Task<DomainError?> CheckAssignableAsync(Guid? ownerCompanyId, Guid? currentOwnerCompanyId, Guid? vehicleId, CancellationToken cancellationToken)
    {
        if (ownerCompanyId is null || ownerCompanyId == currentOwnerCompanyId)
        {
            return null;
        }

        var company = await companies.GetByIdAsync(ownerCompanyId.Value, cancellationToken);
        var error = company is null ? CompanyErrors.Unknown : company.GetSelectionError();
        if (error is not null || company is null || vehicleId is null)
        {
            return error;
        }

        return await rentals.HasCurrentOrUpcomingAsync(vehicleId.Value, company.Id, calendar.Today, cancellationToken)
            ? RentalErrors.OwnerIsTenant(company.Name)
            : null;
    }

    public async Task<VehicleDto> WithOwnerAsync(VehicleDto dto, Vehicle vehicle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(vehicle);

        var byId = (await companies.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(c => c.Id, CompanyDto.From);
        var covering = await rentals.ListCoveringAsync(calendar.Today, [vehicle.Id], cancellationToken);
        return WithOwner(dto, vehicle, covering, byId);
    }

    public async Task<IReadOnlyList<VehicleDto>> WithOwnersAsync(IReadOnlyList<VehicleDto> dtos, IReadOnlyList<Vehicle> vehicles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        ArgumentNullException.ThrowIfNull(vehicles);

        var byId = (await companies.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(c => c.Id, CompanyDto.From);
        var covering = await rentals.ListCoveringAsync(calendar.Today, null, cancellationToken);
        return [.. dtos.Zip(vehicles, (dto, vehicle) => WithOwner(dto, vehicle, covering, byId))];
    }

    private VehicleDto WithOwner(VehicleDto dto, Vehicle vehicle, IReadOnlyList<Rental> covering, Dictionary<Guid, CompanyDto> byId)
    {
        var user = VehicleUserAtDate.Resolve(vehicle.OwnerCompanyId, covering.Where(r => r.VehicleId == vehicle.Id), calendar.Today);
        return dto with
        {
            Owner = vehicle.OwnerCompanyId is { } ownerId ? byId.GetValueOrDefault(ownerId) : null,
            CurrentUser = user.CompanyId is { } userId ? new VehicleUserDto(byId.GetValueOrDefault(userId), user.Source) : VehicleUserDto.NotSet,
        };
    }
}
