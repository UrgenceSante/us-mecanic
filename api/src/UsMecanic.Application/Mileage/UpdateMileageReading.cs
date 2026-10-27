using UsMecanic.Domain.Common;
using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Mileage;

/// <summary>Modifie un relevé avec les mêmes contrôles qu'à l'ajout. L'ancienne valeur n'est pas conservée.</summary>
public sealed class UpdateMileageReading(MileageVehicleGuard guard, IMileageReadingRepository readings, TimeProvider clock)
{
    public async Task<Result<MileageReadingDto>> ExecuteAsync(
        Guid vehicleId, Guid readingId, MileageReadingInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

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

        var all = await readings.ListByVehicleAsync(vehicleId, cancellationToken);
        var updated = reading.Update(input.ToEntry(), all, clock.GetUtcNow());
        if (updated.IsFailure)
        {
            return Result<MileageReadingDto>.Failure(updated.Error!);
        }

        await readings.SaveChangesAsync(cancellationToken);
        return Result<MileageReadingDto>.Success(MileageReadingDto.From(reading));
    }
}
