namespace UsMecanic.Domain.Mileage;

/// <summary>
/// Règle du km actuel : relevé le plus récent par date ; à date égale, le plus élevé en kilomètres
/// (même règle que le « précédent » de <see cref="MileageConsistency"/>), ce qui rend le choix déterministe.
/// </summary>
public static class CurrentMileageRule
{
    /// <summary>Relevé qui donne le km actuel, ou <c>null</c> s'il n'y a aucun relevé.</summary>
    public static MileageReading? Select(IEnumerable<MileageReading> readings)
    {
        ArgumentNullException.ThrowIfNull(readings);

        return readings
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Kilometers)
            .FirstOrDefault();
    }
}
