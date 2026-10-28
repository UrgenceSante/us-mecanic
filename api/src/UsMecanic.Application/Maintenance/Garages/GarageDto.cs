using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Garages;

public sealed record GarageDto(Guid Id, string Name, bool IsActive)
{
    public static GarageDto From(Garage garage)
    {
        ArgumentNullException.ThrowIfNull(garage);

        return new(garage.Id, garage.Name, garage.IsActive);
    }
}
