namespace UsMecanic.Domain.Deadlines;

/// <summary>Seuils « proche » effectivement appliqués à une échéance : ceux de son type, sinon les globaux.</summary>
public sealed record DueSoonLimits(int Days, int Km)
{
    public static DueSoonLimits For(DeadlineType type, DeadlineThresholds global)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(global);

        return new(type.DueSoonDays ?? global.DueSoonDays, type.DueSoonKm ?? global.DueSoonKm);
    }
}
