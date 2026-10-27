using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

public static class DeadlineTypeErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("deadline_type.not_found", "Type d'échéance introuvable.");

    public static readonly DomainError NameRequired =
        new("deadline_type.name_required", "Le nom du type d'échéance est obligatoire.");

    public static readonly DomainError NameTooLong =
        new("deadline_type.name_too_long", $"Le nom du type d'échéance est limité à {DeadlineType.MaxNameLength} caractères.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict("deadline_type.name_already_exists", "Un type d'échéance porte déjà ce nom.");

    public static readonly DomainError IntervalMonthsInvalid =
        new("deadline_type.interval_months_invalid", "La périodicité en mois doit être strictement positive.");

    public static readonly DomainError IntervalKmInvalid =
        new("deadline_type.interval_km_invalid", "La périodicité en kilomètres doit être strictement positive.");

    public static readonly DomainError DueSoonDaysInvalid =
        new("deadline_type.due_soon_days_invalid", "Le seuil « proche » en jours doit être strictement positif.");

    public static readonly DomainError DueSoonKmInvalid =
        new("deadline_type.due_soon_km_invalid", "Le seuil « proche » en kilomètres doit être strictement positif.");
}
