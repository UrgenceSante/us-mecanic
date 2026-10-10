using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>Modifie les caractéristiques d'un véhicule (hors immatriculation et statut).</summary>
public sealed class UpdateVehicleDetails(IVehicleRepository repository, TimeProvider clock)
{
    public async Task<Result<VehicleDto>> ExecuteAsync(Guid id, VehicleDetailsInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var details = input.ToDetails();
        if (details.IsFailure)
        {
            return Result<VehicleDto>.Failure(details.Error!);
        }

        var vehicle = await repository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.NotFound);
        }

        var updated = vehicle.UpdateDetails(details.Value, clock.GetUtcNow());
        if (updated.IsFailure)
        {
            return Result<VehicleDto>.Failure(updated.Error!);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result<VehicleDto>.Success(VehicleDto.From(vehicle));
    }
}
