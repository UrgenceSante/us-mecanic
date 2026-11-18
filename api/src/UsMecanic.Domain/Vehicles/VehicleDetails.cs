namespace UsMecanic.Domain.Vehicles;

/// <summary>Caractéristiques descriptives d'un véhicule, modifiables après son enregistrement.
/// <c>OwnerCompanyId</c> : société propriétaire (facultative ; une fois renseignée, elle ne peut plus être vidée).</summary>
public sealed record VehicleDetails(
    string? Brand,
    string? Model,
    DateOnly? FirstRegistrationDate,
    Vin? Vin,
    string? InternalNumber,
    Guid? TypeId = null,
    Guid? OwnerCompanyId = null);
