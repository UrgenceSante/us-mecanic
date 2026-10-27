using UsMecanic.Application;

namespace UsMecanic.UnitTests.TestDoubles;

internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public static readonly DateTimeOffset Default = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    public static readonly DateOnly DefaultToday = new(2026, 10, 1);

    public DateTimeOffset Now { get; set; } = now;

    /// <summary>Calendrier métier (Europe/Paris) basé sur cette horloge.</summary>
    public BusinessCalendar Calendar => new(this, TimeZoneInfo.FindSystemTimeZoneById(BusinessCalendar.DefaultTimeZoneId));

    public FixedClock()
        : this(Default)
    {
    }

    public override DateTimeOffset GetUtcNow() => Now;
}
