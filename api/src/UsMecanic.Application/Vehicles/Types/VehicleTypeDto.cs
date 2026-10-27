using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles.Types;

public sealed record VehicleTypeDto(Guid Id, string Name, bool IsActive)
{
    public static VehicleTypeDto From(VehicleType vehicleType)
    {
        ArgumentNullException.ThrowIfNull(vehicleType);

        return new(vehicleType.Id, vehicleType.Name, vehicleType.IsActive);
    }
}
