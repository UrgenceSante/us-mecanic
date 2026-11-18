using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Rentals;

/// <summary>
/// Modifie le locataire et les dates d'une location : mêmes contrôles qu'à la création, sauf « sorti du parc »
/// (corriger l'historique reste possible, règle R6).
/// </summary>
public sealed class UpdateRental(IVehicleRepository vehicles, IRentalRepository rentals, RentalWriteChecker checker, RentalDtoFactory factory)
{
    public async Task<Result<RentalDto>> ExecuteAsync(Guid vehicleId, Guid rentalId, RentalInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var vehicle = await vehicles.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle is null)
        {
            return Result<RentalDto>.Failure(VehicleErrors.NotFound);
        }

        var rental = await rentals.GetAsync(vehicleId, rentalId, cancellationToken);
        if (rental is null)
        {
            return Result<RentalDto>.Failure(RentalErrors.NotFound);
        }

        var entry = input.ToEntry();
        var error = await checker.CheckAsync(vehicle, entry, rental, cancellationToken);
        if (error is not null)
        {
            return Result<RentalDto>.Failure(error);
        }

        var updated = rental.Update(entry);
        if (updated.IsFailure)
        {
            return Result<RentalDto>.Failure(updated.Error!);
        }

        await rentals.SaveChangesAsync(cancellationToken);
        return Result<RentalDto>.Success(await factory.ToDtoAsync(rental, cancellationToken));
    }
}
