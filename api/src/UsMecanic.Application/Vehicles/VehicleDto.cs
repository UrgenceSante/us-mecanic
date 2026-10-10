using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

public sealed record VehicleDto(
    Guid Id,
    string LicensePlate,
    string? Brand,
    string? Model,
    DateOnly? FirstRegistrationDate,
    string? Vin,
    string? InternalNumber,
    VehicleStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static VehicleDto From(Vehicle vehicle) => new(
        vehicle.Id,
        vehicle.LicensePlate.Value,
        vehicle.Brand,
        vehicle.Model,
        vehicle.FirstRegistrationDate,
        vehicle.Vin?.Value,
        vehicle.InternalNumber,
        vehicle.Status,
        vehicle.CreatedAt,
        vehicle.UpdatedAt);
}
