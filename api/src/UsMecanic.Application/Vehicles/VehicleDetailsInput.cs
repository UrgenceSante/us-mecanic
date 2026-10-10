using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>Caractéristiques saisies (valeurs brutes, validées par <see cref="ToDetails"/>).</summary>
public sealed record VehicleDetailsInput(
    string? Brand,
    string? Model,
    DateOnly? FirstRegistrationDate,
    string? Vin,
    string? InternalNumber)
{
    public Result<VehicleDetails> ToDetails()
    {
        Vin? vin = null;
        if (!string.IsNullOrWhiteSpace(Vin))
        {
            var parsed = Domain.Vehicles.Vin.Create(Vin);
            if (parsed.IsFailure)
            {
                return Result<VehicleDetails>.Failure(parsed.Error!);
            }

            vin = parsed.Value;
        }

        return Result<VehicleDetails>.Success(new VehicleDetails(Brand, Model, FirstRegistrationDate, vin, InternalNumber));
    }
}
