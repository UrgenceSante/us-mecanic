using UsMecanic.Domain.Common;
using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Mileage;

/// <summary>Ajoute un relevé kilométrique après contrôle de cohérence avec les relevés voisins.</summary>
public sealed class AddMileageReading(MileageVehicleGuard guard, IMileageReadingRepository readings, TimeProvider clock)
{
    public async Task<Result<MileageReadingDto>> ExecuteAsync(Guid vehicleId, MileageReadingInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<MileageReadingDto>.Failure(error);
        }

        var existing = await readings.ListByVehicleAsync(vehicleId, cancellationToken);
        var created = MileageReading.Create(vehicleId, input.ToEntry(), existing, clock.GetUtcNow());
        if (created.IsFailure)
        {
            return Result<MileageReadingDto>.Failure(created.Error!);
        }

        readings.Add(created.Value);
        await readings.SaveChangesAsync(cancellationToken);
        return Result<MileageReadingDto>.Success(MileageReadingDto.From(created.Value));
    }
}
