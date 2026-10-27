using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>Vue d'un véhicule. <c>TypeName</c> reste renseigné même si le type a été désactivé.</summary>
public sealed record VehicleDto(
    Guid Id,
    string LicensePlate,
    string? Brand,
    string? Model,
    DateOnly? FirstRegistrationDate,
    string? Vin,
    string? InternalNumber,
    Guid? TypeId,
    string? TypeName,
    VehicleStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static VehicleDto From(Vehicle vehicle, string? typeName) => new(
        vehicle.Id,
        vehicle.LicensePlate.Value,
        vehicle.Brand,
        vehicle.Model,
        vehicle.FirstRegistrationDate,
        vehicle.Vin?.Value,
        vehicle.InternalNumber,
        vehicle.TypeId,
        typeName,
        vehicle.Status,
        vehicle.CreatedAt,
        vehicle.UpdatedAt);
}
