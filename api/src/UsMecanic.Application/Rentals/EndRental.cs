using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Rentals;

/// <summary>
/// Termine une location sans date de fin au jour indiqué (le front propose la date du jour). Mêmes contrôles qu'une modification :
/// fin ≥ début, et pas de chevauchement. Possible sur un véhicule sorti du parc.
/// </summary>
public sealed class EndRental(IVehicleRepository vehicles, IRentalRepository rentals, RentalWriteChecker checker, RentalDtoFactory factory)
{
    public async Task<Result<RentalDto>> ExecuteAsync(Guid vehicleId, Guid rentalId, DateOnly endDate, CancellationToken cancellationToken)
    {
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

        if (rental.EndDate is not null)
        {
            return Result<RentalDto>.Failure(RentalErrors.AlreadyEnded);
        }

        var entry = new RentalEntry(rental.TenantCompanyId, rental.StartDate, endDate);
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
