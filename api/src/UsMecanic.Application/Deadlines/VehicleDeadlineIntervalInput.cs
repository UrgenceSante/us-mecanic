namespace UsMecanic.Application.Deadlines;

/// <summary>Corps d'une modification de périodicité ; les deux valeurs vides rendent l'échéance ponctuelle.</summary>
public sealed record VehicleDeadlineIntervalInput(int? IntervalMonths, int? IntervalKm);
