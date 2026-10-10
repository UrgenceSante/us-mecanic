namespace UsMecanic.Domain.Vehicles;

/// <summary>Caractéristiques descriptives d'un véhicule, modifiables après son enregistrement.</summary>
public sealed record VehicleDetails(
    string? Brand,
    string? Model,
    DateOnly? FirstRegistrationDate,
    Vin? Vin,
    string? InternalNumber);
