using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

public sealed record DeadlineTypeDto(
    Guid Id,
    string Name,
    int? DefaultIntervalMonths,
    int? DefaultIntervalKm,
    int? DueSoonDays,
    int? DueSoonKm,
    bool IsOneOff,
    bool IsActive)
{
    public static DeadlineTypeDto From(DeadlineType type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return new(type.Id, type.Name, type.DefaultIntervalMonths, type.DefaultIntervalKm, type.DueSoonDays, type.DueSoonKm, type.IsOneOff, type.IsActive);
    }
}
