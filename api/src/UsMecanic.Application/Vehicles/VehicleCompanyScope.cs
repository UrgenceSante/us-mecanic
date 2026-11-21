using UsMecanic.Application.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Véhicules rattachés à une société : ceux dont elle est propriétaire ou utilisatrice du jour (règle R4 : locataire
/// du jour en Europe/Paris, sinon propriétaire). Une seule requête de locations, quel que soit le nombre de véhicules.
/// </summary>
public sealed class VehicleCompanyScope(IRentalRepository rentals, BusinessCalendar calendar)
{
    public async Task<HashSet<Guid>> VehicleIdsAsync(IEnumerable<Vehicle> vehicles, Guid companyId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(vehicles);

        var today = calendar.Today;
        var covering = (await rentals.ListCoveringAsync(today, null, cancellationToken)).ToLookup(r => r.VehicleId);
        return [.. vehicles
            .Where(v => v.OwnerCompanyId == companyId
                || VehicleUserAtDate.Resolve(v.OwnerCompanyId, covering[v.Id], today).CompanyId == companyId)
            .Select(v => v.Id)];
    }
}
