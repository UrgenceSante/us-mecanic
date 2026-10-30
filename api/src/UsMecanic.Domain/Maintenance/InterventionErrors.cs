using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Maintenance;

public static class InterventionErrors
{
    /// <summary>Intervention inexistante ou appartenant à un autre véhicule.</summary>
    public static readonly DomainError NotFound =
        DomainError.NotFound("intervention.not_found", "Intervention introuvable.");

    public static readonly DomainError WorkRequired =
        new("intervention.work_required", "Les travaux réalisés sont obligatoires.");

    public static readonly DomainError WorkTooLong =
        new("intervention.work_too_long", $"Les travaux sont limités à {Intervention.MaxWorkLength} caractères.");

    public static readonly DomainError DateInFuture =
        new("intervention.date_in_future", "La date de l'intervention ne peut pas être dans le futur.");

    public static readonly DomainError DeadlineDuplicated =
        new("intervention.deadline_duplicated", "Une échéance ne peut être cochée qu'une seule fois.");

    /// <summary>Échéance inexistante ou appartenant à un autre véhicule.</summary>
    public static readonly DomainError DeadlineUnknown =
        new("intervention.deadline_unknown", "Échéance inconnue pour ce véhicule.");

    public static readonly DomainError DeadlineClosed =
        DomainError.Conflict("intervention.deadline_closed", "Une échéance close ne peut pas être réalisée par une intervention.");

    public static readonly DomainError AmountNegative =
        new("intervention.amount_negative", "Le montant ne peut pas être négatif.");

    public static readonly DomainError AmountTooPrecise =
        new("intervention.amount_too_precise", "Le montant est limité à 2 décimales.");

    public static readonly DomainError GarageUnknown =
        new("intervention.garage_unknown", "Garage inconnu.");

    public static readonly DomainError GarageInactive =
        new("intervention.garage_inactive", "Ce garage est désactivé : il ne peut plus être affecté à une intervention.");
}
