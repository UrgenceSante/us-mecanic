using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Deadlines;

/// <summary>
/// Marque une échéance réalisée en créant une intervention liée à cette seule échéance (travaux = commentaire s'il est
/// renseigné, sinon nom du type d'échéance). Les contrôles et l'enregistrement (relevé km, échéance, intervention,
/// une seule transaction) sont ceux de <see cref="AddIntervention"/>.
/// </summary>
public sealed class CompleteVehicleDeadline(
    MileageVehicleGuard guard,
    IVehicleDeadlineRepository deadlines,
    IDeadlineTypeRepository types,
    AddIntervention addIntervention,
    VehicleDeadlineDtoFactory factory)
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

        var comment = string.IsNullOrWhiteSpace(input.Comment) ? null : input.Comment.Trim();
        if (comment?.Length > DeadlineCompletionErrors.MaxCommentLength)
        {
            return Result<VehicleDeadlineDto>.Failure(DeadlineCompletionErrors.CommentTooLong);
        }

        var work = comment ?? (await types.GetByIdAsync(deadline.DeadlineTypeId, cancellationToken))?.Name;
        var intervention = new InterventionInput(
            input.Date, input.Kilometers, input.GarageId, work, [deadline.Id], input.Reason, input.ConfirmHighMileage, input.Amount);
        var created = await addIntervention.ExecuteAsync(vehicleId, intervention, cancellationToken);
        if (created.IsFailure)
        {
            // Même code d'erreur qu'avant l'introduction des interventions.
            return Result<VehicleDeadlineDto>.Failure(
                created.Error == InterventionErrors.DateInFuture ? DeadlineCompletionErrors.DateInFuture : created.Error!);
        }

        return Result<VehicleDeadlineDto>.Success((await factory.BuildAsync(vehicleId, [deadline], cancellationToken))[0]);
    }
}
