using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles.Types;

/// <summary>Ajoute un type de véhicule. Le nom est obligatoire et unique (sans tenir compte de la casse).</summary>
public sealed class CreateVehicleType(IVehicleTypeRepository repository)
{
    public async Task<Result<VehicleTypeDto>> ExecuteAsync(string? name, CancellationToken cancellationToken)
    {
        var vehicleType = VehicleType.Create(name);
        if (vehicleType.IsFailure)
        {
            return Result<VehicleTypeDto>.Failure(vehicleType.Error!);
        }

        if (await repository.NameExistsAsync(vehicleType.Value.Name, null, cancellationToken))
        {
            return Result<VehicleTypeDto>.Failure(VehicleTypeErrors.NameAlreadyExists);
        }

        repository.Add(vehicleType.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<VehicleTypeDto>.Success(VehicleTypeDto.From(vehicleType.Value));
    }
}
