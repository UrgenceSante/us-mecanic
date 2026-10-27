using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Vue d'un véhicule. <c>TypeName</c> reste renseigné même si le type a été désactivé.
/// <c>CurrentMileage</c> et <c>CurrentMileageDate</c> viennent du relevé le plus récent (null sans relevé).
/// </summary>
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
    DateTimeOffset UpdatedAt,
    int? CurrentMileage,
    DateOnly? CurrentMileageDate)
{
    public static VehicleDto From(Vehicle vehicle, string? typeName, CurrentMileage? mileage) => new(
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
        vehicle.UpdatedAt,
        mileage?.Kilometers,
        mileage?.Date);
}
