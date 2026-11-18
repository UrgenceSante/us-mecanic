using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Rentals;

/// <summary>Supprime une location saisie par erreur (possible sur un véhicule sorti du parc). Renvoie la location supprimée.</summary>
public sealed class DeleteRental(IVehicleRepository vehicles, IRentalRepository rentals, RentalDtoFactory factory)
{
    public async Task<Result<RentalDto>> ExecuteAsync(Guid vehicleId, Guid rentalId, CancellationToken cancellationToken)
    {
        if (await vehicles.GetByIdAsync(vehicleId, cancellationToken) is null)
        {
            return Result<RentalDto>.Failure(VehicleErrors.NotFound);
        }

        var rental = await rentals.GetAsync(vehicleId, rentalId, cancellationToken);
        if (rental is null)
        {
            return Result<RentalDto>.Failure(RentalErrors.NotFound);
        }

        var dto = await factory.ToDtoAsync(rental, cancellationToken);
        rentals.Remove(rental);
        await rentals.SaveChangesAsync(cancellationToken);
        return Result<RentalDto>.Success(dto);
    }
}
