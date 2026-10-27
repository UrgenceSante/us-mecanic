using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles.Types;

/// <summary>Renomme un type de véhicule. Le nouveau nom doit rester unique.</summary>
public sealed class RenameVehicleType(IVehicleTypeRepository repository)
{
    public async Task<Result<VehicleTypeDto>> ExecuteAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        var normalized = VehicleType.NormalizeName(name);
        if (normalized.IsFailure)
        {
            return Result<VehicleTypeDto>.Failure(normalized.Error!);
        }

        var vehicleType = await repository.GetByIdAsync(id, cancellationToken);
        if (vehicleType is null)
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NotFound);
        }

        if (await repository.NameExistsAsync(normalized.Value, id, cancellationToken))
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NameAlreadyExists);
        }

        vehicleType.Rename(normalized.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<VehicleTypeDto>.Success(VehicleTypeDto.From(vehicleType));
    }
}
