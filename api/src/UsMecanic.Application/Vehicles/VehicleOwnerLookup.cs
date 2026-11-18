using UsMecanic.Application.Companies;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Companies;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Relie les véhicules à leur société propriétaire : contrôle qu'une société peut être attribuée et fournit
/// propriétaire et utilisateur actuel aux DTO (une société désactivée reste affichée sur les véhicules qui la citent).
/// </summary>
public sealed class VehicleOwnerLookup(ICompanyRepository companies)
{
    /// <summary>
    /// Une société doit exister et être active pour être choisie ; un véhicule peut en revanche conserver
    /// son propriétaire actuel même désactivé. Renvoie <c>null</c> si l'attribution est permise.
    /// Le contrôle « pas un locataire en cours » viendra avec les locations (issue #209).
    /// </summary>
    public async Task<DomainError?> CheckAssignableAsync(Guid? ownerCompanyId, Guid? currentOwnerCompanyId, CancellationToken cancellationToken)
    {
        if (ownerCompanyId is null || ownerCompanyId == currentOwnerCompanyId)
        {
            return null;
        }

        var company = await companies.GetByIdAsync(ownerCompanyId.Value, cancellationToken);
        return company is null ? CompanyErrors.Unknown : company.GetSelectionError();
    }

    public async Task<VehicleDto> WithOwnerAsync(VehicleDto dto, Vehicle vehicle, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(vehicle);

        var owner = vehicle.OwnerCompanyId is { } id ? await companies.GetByIdAsync(id, cancellationToken) : null;
        return WithOwner(dto, owner is null ? null : CompanyDto.From(owner));
    }

    public async Task<IReadOnlyList<VehicleDto>> WithOwnersAsync(IReadOnlyList<VehicleDto> dtos, IReadOnlyList<Vehicle> vehicles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dtos);
        ArgumentNullException.ThrowIfNull(vehicles);

        var byId = (await companies.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(c => c.Id, CompanyDto.From);
        return [.. dtos.Zip(vehicles, (dto, vehicle) => WithOwner(dto, vehicle.OwnerCompanyId is { } id ? byId.GetValueOrDefault(id) : null))];
    }

    private static VehicleDto WithOwner(VehicleDto dto, CompanyDto? owner) =>
        dto with { Owner = owner, CurrentUser = VehicleUserDto.Resolve(owner, tenant: null) };
}
