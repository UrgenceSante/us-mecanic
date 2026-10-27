namespace UsMecanic.Domain.Deadlines;

/// <summary>
/// Calcul pur du statut d'une échéance : le premier critère (date ou km) atteint l'emporte.
/// Dépassée : date strictement passée, ou km actuel supérieur ou égal au km d'échéance.
/// Proche : date à <see cref="DueSoonLimits.Days"/> jours ou moins (jour même inclus), ou km restants
/// inférieurs ou égaux à <see cref="DueSoonLimits.Km"/>. Sans km actuel, le critère km est ignoré.
/// </summary>
public static class DeadlineStatusCalculator
{
    public static DeadlineStatus Compute(DateOnly? dueDate, int? dueKm, int? currentKm, DateOnly today, DueSoonLimits limits)
    {
        ArgumentNullException.ThrowIfNull(limits);

        var daysLeft = dueDate is { } date ? date.DayNumber - today.DayNumber : (int?)null;
        var kmLeft = dueKm is { } km && currentKm is { } current ? km - current : (int?)null;

        if (daysLeft < 0 || kmLeft <= 0)
        {
            return DeadlineStatus.Overdue;
        }

        return daysLeft <= limits.Days || kmLeft <= limits.Km ? DeadlineStatus.DueSoon : DeadlineStatus.UpToDate;
    }
}
