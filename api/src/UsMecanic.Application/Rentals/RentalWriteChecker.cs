using UsMecanic.Application.Companies;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Companies;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Rentals;

/// <summary>
/// Contrôles communs à la création et à la modification d'une location (US3, US4) : saisie valide, véhicule avec propriétaire,
/// locataire différent du propriétaire et actif, pas de chevauchement avec une autre location du véhicule.
/// </summary>
public sealed class RentalWriteChecker(ICompanyRepository companies, IRentalRepository rentals)
{
    /// <param name="vehicle">Véhicule loué.</param>
    /// <param name="entry">Valeurs à enregistrer.</param>
    /// <param name="existing">Location modifiée ; <c>null</c> pour une création (seule une création est refusée hors parc).</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'erreur à renvoyer, ou <c>null</c> si l'écriture est permise.</returns>
    public async Task<DomainError?> CheckAsync(Vehicle vehicle, RentalEntry entry, Rental? existing, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(entry);

        var error = Rental.Validate(entry) ?? CheckVehicle(vehicle, entry, existing);
        if (error is not null)
        {
            return error;
        }

        // Un locataire conservé tel quel reste accepté même désactivé ; un nouveau locataire doit exister et être actif.
        if (existing is null || existing.TenantCompanyId != entry.TenantCompanyId)
        {
            var tenant = await companies.GetByIdAsync(entry.TenantCompanyId, cancellationToken);
            error = tenant is null ? CompanyErrors.Unknown : tenant.GetSelectionError();
            if (error is not null)
            {
                return error;
            }
        }

        return await CheckOverlapAsync(vehicle.Id, entry, existing, cancellationToken);
    }

    private static DomainError? CheckVehicle(Vehicle vehicle, RentalEntry entry, Rental? existing)
    {
        if (existing is null && vehicle.Status == VehicleStatus.Retired)
        {
            return RentalErrors.VehicleRetired;
        }

        if (vehicle.OwnerCompanyId is not { } owner)
        {
            return RentalErrors.OwnerRequired;
        }

        return owner == entry.TenantCompanyId ? RentalErrors.TenantIsOwner : null;
    }

    private async Task<DomainError?> CheckOverlapAsync(Guid vehicleId, RentalEntry entry, Rental? existing, CancellationToken cancellationToken)
    {
        var others = await rentals.ListByVehicleAsync(vehicleId, cancellationToken);
        var conflict = others.FirstOrDefault(r => r.Id != existing?.Id && r.Overlaps(entry.StartDate, entry.EndDate));
        if (conflict is null)
        {
            return null;
        }

        var tenant = await companies.GetByIdAsync(conflict.TenantCompanyId, cancellationToken);
        return RentalErrors.Overlap(tenant?.Name ?? "une société inconnue", conflict.StartDate, conflict.EndDate);
    }
}
