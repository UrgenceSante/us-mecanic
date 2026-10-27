using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Mileage;

/// <summary>Liste les relevés d'un véhicule, du plus récent au plus ancien (consultable même hors parc).</summary>
public sealed class ListMileageReadings(MileageVehicleGuard guard, IMileageReadingRepository readings)
{
    public async Task<Result<IReadOnlyList<MileageReadingDto>>> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<MileageReadingDto>>.Failure(error);
        }

        var list = await readings.ListByVehicleAsync(vehicleId, cancellationToken);
        return Result<IReadOnlyList<MileageReadingDto>>.Success([.. list.Select(MileageReadingDto.From)]);
    }
}
