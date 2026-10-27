using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

public sealed record DeadlineThresholdsDto(int DueSoonDays, int DueSoonKm)
{
    public static DeadlineThresholdsDto From(DeadlineThresholds thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        return new(thresholds.DueSoonDays, thresholds.DueSoonKm);
    }
}
