namespace UsMecanic.Application;

/// <summary>
/// Calendrier métier : calcule la « date du jour » dans le fuseau horaire de l'exploitation
/// (et non en UTC). Les horodatages stockés restent en UTC.
/// </summary>
public sealed class BusinessCalendar(TimeProvider clock, TimeZoneInfo zone)
{
    /// <summary>Fuseau métier par défaut.</summary>
    public const string DefaultTimeZoneId = "Europe/Paris";

    /// <summary>Date du jour dans le fuseau métier.</summary>
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), zone).DateTime);
}
