using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Mileage;

/// <summary>
/// Contrôles de cohérence d'un relevé par rapport aux relevés voisins, dans l'ordre des dates.
/// Précédent = relevé le plus récent à la même date ou avant ; suivant = relevé le plus ancien après.
/// </summary>
public static class MileageConsistency
{
    public const int MaxKilometersPerDay = 1000;

    /// <summary>Renvoie la première règle violée, ou <c>null</c> si le relevé est cohérent.</summary>
    /// <param name="entry">Valeurs saisies (motif déjà nettoyé).</param>
    /// <param name="others">Autres relevés du véhicule (hors relevé en cours de modification).</param>
    /// <param name="today">Jour courant, au-delà duquel une date est dans le futur.</param>
    public static DomainError? Check(MileageReadingEntry entry, IEnumerable<MileageReading> others, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(others);

        if (entry.Kilometers < 0)
        {
            return MileageReadingErrors.NegativeKilometers;
        }

        if (entry.Date > today)
        {
            return MileageReadingErrors.DateInFuture;
        }

        var readings = others.ToList();
        var previous = readings
            .Where(r => r.Date <= entry.Date)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Kilometers)
            .FirstOrDefault();
        var next = readings
            .Where(r => r.Date > entry.Date)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.Kilometers)
            .FirstOrDefault();

        var outOfOrder = entry.Kilometers < previous?.Kilometers || entry.Kilometers > next?.Kilometers;
        if (outOfOrder)
        {
            return string.IsNullOrWhiteSpace(entry.Reason) ? MileageReadingErrors.ReasonRequired : null;
        }

        return previous is not null && !entry.ConfirmHighMileage && ExceedsDailyLimit(entry, previous)
            ? MileageReadingErrors.HighMileageConfirmationRequired
            : null;
    }

    /// <summary>Moyenne journalière depuis le précédent ; un écart de 0 jour compte pour 1 jour.</summary>
    private static bool ExceedsDailyLimit(MileageReadingEntry entry, MileageReading previous)
    {
        long days = Math.Max(entry.Date.DayNumber - previous.Date.DayNumber, 1);
        return entry.Kilometers - (long)previous.Kilometers > MaxKilometersPerDay * days;
    }
}
