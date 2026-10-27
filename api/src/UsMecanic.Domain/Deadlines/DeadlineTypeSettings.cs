namespace UsMecanic.Domain.Deadlines;

/// <summary>
/// Périodicité par défaut et seuils « proche » propres à un type d'échéance.
/// Sans périodicité (mois et km nuls), le type est ponctuel. Un seuil nul signifie « seuils globaux ».
/// </summary>
public sealed record DeadlineTypeSettings(int? IntervalMonths, int? IntervalKm, int? DueSoonDays, int? DueSoonKm);
