using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Rentals;

/// <summary>Enregistre une location (passée, en cours ou prévue) d'un véhicule du parc.</summary>
public sealed class AddRental(IVehicleRepository vehicles, IRentalRepository rentals, RentalWriteChecker checker, RentalDtoFactory factory)
{
    public async Task<Result<RentalDto>> ExecuteAsync(Guid vehicleId, RentalInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var vehicle = await vehicles.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result<RentalDto>.Failure(VehicleErrors.NotFound);
        }

        var entry = input.ToEntry();
        var error = await checker.CheckAsync(vehicle, entry, existing: null, cancellationToken);
        if (error is not null)
        {
            return Result<RentalDto>.Failure(error);
        }

        var created = Rental.Create(vehicleId, entry);
        if (created.IsFailure)
        {
            return Result<RentalDto>.Failure(created.Error!);
        }

        rentals.Add(created.Value);
        await rentals.SaveChangesAsync(cancellationToken);
        return Result<RentalDto>.Success(await factory.ToDtoAsync(created.Value, cancellationToken));
    }
}
