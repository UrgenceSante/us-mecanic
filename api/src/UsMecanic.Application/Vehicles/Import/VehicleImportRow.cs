namespace UsMecanic.Application.Vehicles.Import;

/// <summary>Ligne brute d'une source d'import. <see cref="Line"/> est le numéro de ligne dans la source.</summary>
public sealed record VehicleImportRow(
    int Line,
    string? LicensePlate,
    string? Brand = null,
    string? Model = null,
    string? FirstRegistrationDate = null,
    string? Vin = null,
    string? InternalNumber = null);
