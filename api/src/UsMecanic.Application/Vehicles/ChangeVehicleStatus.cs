using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>Met un véhicule en service, l'immobilise ou le sort du parc.</summary>
public sealed class ChangeVehicleStatus(IVehicleRepository repository, VehicleTypeLookup typeLookup, TimeProvider clock)
{
    public async Task<Result<VehicleDto>> ExecuteAsync(Guid id, VehicleStatus status, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.NotFound);
        }

        var changed = vehicle.ChangeStatus(status, clock.GetUtcNow());
        if (changed.IsFailure)
        {
            return Result<VehicleDto>.Failure(changed.Error!);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result<VehicleDto>.Success(await typeLookup.ToDtoAsync(vehicle, cancellationToken));
    }
}
