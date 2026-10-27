using UsMecanic.Domain.Common;
using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Mileage;

/// <summary>Supprime un relevé, sans trace de l'ancienne valeur. Renvoie le relevé supprimé.</summary>
public sealed class DeleteMileageReading(MileageVehicleGuard guard, IMileageReadingRepository readings)
{
    public async Task<Result<MileageReadingDto>> ExecuteAsync(Guid vehicleId, Guid readingId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<MileageReadingDto>.Failure(error);
        }

        var reading = await readings.GetAsync(vehicleId, readingId, cancellationToken);
        if (reading is null)
        {
            return Result<MileageReadingDto>.Failure(MileageReadingErrors.NotFound);
        }

        readings.Remove(reading);
        await readings.SaveChangesAsync(cancellationToken);
        return Result<MileageReadingDto>.Success(MileageReadingDto.From(reading));
    }
}
