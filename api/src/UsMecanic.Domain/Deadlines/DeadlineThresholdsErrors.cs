using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

public static class DeadlineThresholdsErrors
{
    public static readonly DomainError DaysInvalid =
        new("deadline_thresholds.days_invalid", "Le seuil « proche » en jours est obligatoire et doit être strictement positif.");

    public static readonly DomainError KmInvalid =
        new("deadline_thresholds.km_invalid", "Le seuil « proche » en kilomètres est obligatoire et doit être strictement positif.");
}
