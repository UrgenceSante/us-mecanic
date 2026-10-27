using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

public sealed class GetVehicle(IVehicleRepository repository, VehicleTypeLookup typeLookup)
{
    public async Task<Result<VehicleDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(id, cancellationToken);
        return vehicle is null
            ? Result<VehicleDto>.Failure(VehicleErrors.NotFound)
            : Result<VehicleDto>.Success(await typeLookup.ToDtoAsync(vehicle, cancellationToken));
    }
}
