using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Ajoute une échéance à un véhicule : type actif, une seule échéance non close par type et par véhicule.</summary>
public sealed class AddVehicleDeadline(
    MileageVehicleGuard guard,
    IDeadlineTypeRepository types,
    IVehicleDeadlineRepository deadlines,
    VehicleDeadlineDtoFactory factory,
    BusinessCalendar calendar)
{
    public async Task<Result<VehicleDeadlineDto>> ExecuteAsync(Guid vehicleId, VehicleDeadlineInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<VehicleDeadlineDto>.Failure(error);
        }

        var type = await types.GetByIdAsync(input.DeadlineTypeId, cancellationToken);
        if (type is null)
        {
            return Result<VehicleDeadlineDto>.Failure(VehicleDeadlineErrors.TypeUnknown);
        }

        var today = calendar.Today;
        var created = VehicleDeadline.Create(vehicleId, type, input.ToSchedule(), today);
        if (created.IsFailure)
        {
            return Result<VehicleDeadlineDto>.Failure(created.Error!);
        }

        if (await deadlines.ActiveExistsAsync(vehicleId, type.Id, cancellationToken))
        {
            return Result<VehicleDeadlineDto>.Failure(VehicleDeadlineErrors.AlreadyActive);
        }

        deadlines.Add(created.Value);
        await deadlines.SaveChangesAsync(cancellationToken);
        return Result<VehicleDeadlineDto>.Success((await factory.BuildAsync(vehicleId, [created.Value], cancellationToken))[0]);
    }
}
