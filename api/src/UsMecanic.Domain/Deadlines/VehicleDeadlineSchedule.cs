namespace UsMecanic.Domain.Deadlines;

/// <summary>
/// Planification saisie pour l'échéance d'un véhicule. Avec au moins une périodicité (mois ou km),
/// l'échéance est périodique : date et km d'échéance sont calculés depuis la dernière réalisation et
/// <paramref name="DueDate"/> / <paramref name="DueKm"/> sont ignorés. Sans périodicité, elle est
/// ponctuelle et date et/ou km d'échéance sont saisis directement.
/// </summary>
public sealed record VehicleDeadlineSchedule(
    int? IntervalMonths,
    int? IntervalKm,
    DateOnly? LastDoneDate,
    int? LastDoneKm,
    DateOnly? DueDate,
    int? DueKm)
{
    public bool IsPeriodic => IntervalMonths is not null || IntervalKm is not null;
}
