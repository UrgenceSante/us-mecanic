using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Modifie la périodicité d'une échéance et recalcule sa prochaine échéance depuis la dernière réalisation.</summary>
public sealed class UpdateVehicleDeadlineInterval(
    MileageVehicleGuard guard,
    IVehicleDeadlineRepository deadlines,
    VehicleDeadlineDtoFactory factory,
    TimeProvider clock)
{
    public async Task<Result<VehicleDeadlineDto>> ExecuteAsync(
        Guid vehicleId, Guid deadlineId, VehicleDeadlineIntervalInput input, CancellationToken cancellationToken)
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

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        var changed = deadline.ChangeInterval(input.IntervalMonths, input.IntervalKm, today);
        if (changed.IsFailure)
        {
            return Result<VehicleDeadlineDto>.Failure(changed.Error!);
        }

        await deadlines.SaveChangesAsync(cancellationToken);
        return Result<VehicleDeadlineDto>.Success((await factory.BuildAsync(vehicleId, [deadline], cancellationToken))[0]);
    }
}
