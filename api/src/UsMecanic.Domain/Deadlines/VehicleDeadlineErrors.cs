using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

public static class VehicleDeadlineErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("vehicle_deadline.not_found", "Échéance introuvable.");

    public static readonly DomainError TypeUnknown =
        new("vehicle_deadline.type_unknown", "Type d'échéance inconnu.");

    public static readonly DomainError TypeInactive =
        new("vehicle_deadline.type_inactive", "Ce type d'échéance est désactivé : il ne peut plus être ajouté à un véhicule.");

    public static readonly DomainError AlreadyActive =
        DomainError.Conflict("vehicle_deadline.already_active", "Ce véhicule a déjà une échéance en cours de ce type.");

    public static readonly DomainError Closed =
        DomainError.Conflict("vehicle_deadline.closed", "Cette échéance est close : elle ne peut plus être modifiée.");

    public static readonly DomainError CriterionRequired =
        new("vehicle_deadline.criterion_required", "Au moins un critère est requis : une date ou un km d'échéance (ponctuelle), ou une dernière réalisation correspondant à la périodicité (périodique).");

    public static readonly DomainError IntervalMonthsInvalid =
        new("vehicle_deadline.interval_months_invalid", $"La périodicité en mois doit être comprise entre 1 et {VehicleDeadline.MaxIntervalMonths}.");

    public static readonly DomainError IntervalKmInvalid =
        new("vehicle_deadline.interval_km_invalid", $"La périodicité en kilomètres doit être comprise entre 1 et {VehicleDeadline.MaxKilometers}.");

    public static readonly DomainError KilometersInvalid =
        new("vehicle_deadline.kilometers_invalid", $"Les kilométrages doivent être compris entre 0 et {VehicleDeadline.MaxKilometers}.");

    public static readonly DomainError LastDoneInFuture =
        new("vehicle_deadline.last_done_in_future", "La date de dernière réalisation ne peut pas être dans le futur.");

    public static readonly DomainError OneOffNotAssignable =
        new("vehicle_deadline.one_off_not_assignable", "Un type d'échéance ponctuel (sans périodicité) ne peut pas être ajouté en lot : sa date d'échéance dépend de chaque véhicule.");

    public static readonly DomainError VehicleTypesRequired =
        new("vehicle_deadline.vehicle_types_required", "Au moins un type de véhicule est requis.");
}
