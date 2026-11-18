using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Rentals;

/// <summary>Liste les locations d'un véhicule, de la plus récente à la plus ancienne (consultable même hors parc).</summary>
public sealed class ListRentals(IVehicleRepository vehicles, IRentalRepository rentals, RentalDtoFactory factory)
{
    public async Task<Result<IReadOnlyList<RentalDto>>> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        if (await vehicles.GetByIdAsync(vehicleId, cancellationToken) is null)
        {
            return Result<IReadOnlyList<RentalDto>>.Failure(VehicleErrors.NotFound);
        }

        var list = await rentals.ListByVehicleAsync(vehicleId, cancellationToken);
        return Result<IReadOnlyList<RentalDto>>.Success(await factory.ToDtosAsync(list, cancellationToken));
    }
}
