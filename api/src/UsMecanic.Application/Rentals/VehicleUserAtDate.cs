using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Rentals;

namespace UsMecanic.Application.Rentals;

/// <summary>Utilisateur d'un véhicule à une date : identifiant de la société (null si non renseigné) et origine.</summary>
public sealed record VehicleUserAtDateResult(Guid? CompanyId, VehicleUserSource Source);

/// <summary>
/// Règle R4 de la phase 5 bis, pure et réutilisable (liste du parc, fiche véhicule, synthèse des coûts) :
/// l'utilisateur à une date est le locataire de la location qui couvre cette date, sinon le propriétaire courant,
/// sinon « non renseigné ».
/// </summary>
public static class VehicleUserAtDate
{
    /// <param name="ownerCompanyId">Propriétaire courant du véhicule, s'il est renseigné.</param>
    /// <param name="rentals">Locations du véhicule (celles d'autres véhicules ne doivent pas être passées).</param>
    /// <param name="day">Jour concerné.</param>
    public static VehicleUserAtDateResult Resolve(Guid? ownerCompanyId, IEnumerable<Rental> rentals, DateOnly day)
    {
        ArgumentNullException.ThrowIfNull(rentals);

        var covering = rentals.FirstOrDefault(r => r.Covers(day));
        if (covering is not null)
        {
            return new(covering.TenantCompanyId, VehicleUserSource.Tenant);
        }

        return ownerCompanyId is { } owner
            ? new(owner, VehicleUserSource.Owner)
            : new(null, VehicleUserSource.NotSet);
    }
}
