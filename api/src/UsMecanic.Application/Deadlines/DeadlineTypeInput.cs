using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Données saisies pour créer ou modifier un type d'échéance.</summary>
public sealed record DeadlineTypeInput(string? Name, int? DefaultIntervalMonths, int? DefaultIntervalKm, int? DueSoonDays, int? DueSoonKm)
{
    public DeadlineTypeSettings Settings => new(DefaultIntervalMonths, DefaultIntervalKm, DueSoonDays, DueSoonKm);
}
