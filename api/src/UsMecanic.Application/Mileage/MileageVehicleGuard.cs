using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Mileage;

/// <summary>Vérifie qu'un véhicule existe et, pour une écriture, qu'il n'est pas sorti du parc.</summary>
public sealed class MileageVehicleGuard(IVehicleRepository vehicles)
{
    /// <returns>L'erreur à renvoyer, ou <c>null</c> si le véhicule est utilisable.</returns>
    public async Task<DomainError?> CheckAsync(Guid vehicleId, bool forWrite, CancellationToken cancellationToken)
    {
        var vehicle = await vehicles.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle is null)
        {
            return VehicleErrors.NotFound;
        }

        return forWrite && vehicle.Status == VehicleStatus.Retired ? VehicleErrors.Retired : null;
    }
}
