using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Échéance d'un véhicule ; <paramref name="Status"/> est nul pour une échéance close.</summary>
public sealed record VehicleDeadlineDto(
    Guid Id,
    Guid VehicleId,
    Guid DeadlineTypeId,
    string? TypeName,
    int? IntervalMonths,
    int? IntervalKm,
    DateOnly? LastDoneDate,
    int? LastDoneKm,
    DateOnly? DueDate,
    int? DueKm,
    bool IsClosed,
    DeadlineStatus? Status)
{
    public static VehicleDeadlineDto From(VehicleDeadline deadline, string? typeName, DeadlineStatus? status)
    {
        ArgumentNullException.ThrowIfNull(deadline);

        return new(
            deadline.Id,
            deadline.VehicleId,
            deadline.DeadlineTypeId,
            typeName,
            deadline.IntervalMonths,
            deadline.IntervalKm,
            deadline.LastDoneDate,
            deadline.LastDoneKm,
            deadline.DueDate,
            deadline.DueKm,
            deadline.IsClosed,
            status);
    }
}
