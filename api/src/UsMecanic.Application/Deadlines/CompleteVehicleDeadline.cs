using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Deadlines;

/// <summary>
/// Marque une échéance réalisée. Un km fourni crée un relevé kilométrique avec les mêmes contrôles que
/// <see cref="AddMileageReading"/> ; relevé, réalisation et échéance sont enregistrés ensemble (une seule transaction).
/// </summary>
public sealed class CompleteVehicleDeadline(
    MileageVehicleGuard guard,
    IVehicleDeadlineRepository deadlines,
    IDeadlineCompletionRepository completions,
    IMileageReadingRepository readings,
    VehicleDeadlineDtoFactory factory,
    TimeProvider clock)
{
    public async Task<Result<VehicleDeadlineDto>> ExecuteAsync(
        Guid vehicleId, Guid deadlineId, DeadlineCompletionInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<VehicleDeadlineDto>.Failure(error);
        }

        var deadline = await deadlines.GetAsync(vehicleId, deadlineId, cancellationToken);
        if (deadline is null)
        {
            return Result<VehicleDeadlineDto>.Failure(VehicleDeadlineErrors.NotFound);
        }

        if (deadline.IsClosed)
        {
            return Result<VehicleDeadlineDto>.Failure(VehicleDeadlineErrors.Closed);
        }

        var now = clock.GetUtcNow();
        var completion = DeadlineCompletion.Create(deadline.Id, input.Date, input.Kilometers, input.Comment, now);
        if (completion.IsFailure)
        {
            return Result<VehicleDeadlineDto>.Failure(completion.Error!);
        }

        var reading = await CreateReadingAsync(vehicleId, input, now, cancellationToken);
        if (reading.IsFailure)
        {
            return Result<VehicleDeadlineDto>.Failure(reading.Error!);
        }

        var done = deadline.Complete(input.Date, input.Kilometers, DateOnly.FromDateTime(now.UtcDateTime));
        if (done.IsFailure)
        {
            return Result<VehicleDeadlineDto>.Failure(done.Error!);
        }

        completions.Add(completion.Value);
        if (reading.Value is not null)
        {
            readings.Add(reading.Value);
        }

        await deadlines.SaveChangesAsync(cancellationToken);
        return Result<VehicleDeadlineDto>.Success((await factory.BuildAsync(vehicleId, [deadline], cancellationToken))[0]);
    }

    /// <summary>Relevé à créer (mêmes contrôles qu'un ajout de relevé), ou <c>null</c> sans km.</summary>
    private async Task<Result<MileageReading?>> CreateReadingAsync(
        Guid vehicleId, DeadlineCompletionInput input, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (input.Kilometers is not { } kilometers)
        {
            return Result<MileageReading?>.Success(null);
        }

        var existing = await readings.ListByVehicleAsync(vehicleId, cancellationToken);
        var entry = new MileageReadingEntry(input.Date, kilometers, input.Reason, input.ConfirmHighMileage);
        var created = MileageReading.Create(vehicleId, entry, existing, now);
        return created.IsSuccess
            ? Result<MileageReading?>.Success(created.Value)
            : Result<MileageReading?>.Failure(created.Error!);
    }
}
