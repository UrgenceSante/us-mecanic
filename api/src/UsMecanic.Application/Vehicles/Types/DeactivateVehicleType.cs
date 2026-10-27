using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles.Types;

/// <summary>
/// Désactive un type : il n'est plus proposé mais reste affiché sur les véhicules qui l'ont.
/// Il n'existe volontairement aucune suppression de type.
/// </summary>
public sealed class DeactivateVehicleType(IVehicleTypeRepository repository)
{
    public async Task<Result<VehicleTypeDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var vehicleType = await repository.GetByIdAsync(id, cancellationToken);
        if (vehicleType is null)
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NotFound);
        }

        vehicleType.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return Result<VehicleTypeDto>.Success(VehicleTypeDto.From(vehicleType));
    }
}
