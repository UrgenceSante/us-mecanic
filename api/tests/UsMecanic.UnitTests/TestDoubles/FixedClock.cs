namespace UsMecanic.UnitTests.TestDoubles;

internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public static readonly DateTimeOffset Default = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    public DateTimeOffset Now { get; set; } = now;

    public FixedClock()
        : this(Default)
    {
    }

    public override DateTimeOffset GetUtcNow() => Now;
}
