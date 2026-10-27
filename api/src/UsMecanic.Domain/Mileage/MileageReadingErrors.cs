using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Mileage;

public static class MileageReadingErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("mileage_reading.not_found", "Relevé kilométrique introuvable.");

    public static readonly DomainError NegativeKilometers =
        new("mileage_reading.kilometers_negative", "Le kilométrage ne peut pas être négatif.");

    public static readonly DomainError DateInFuture =
        new("mileage_reading.date_in_future", "La date du relevé ne peut pas être dans le futur.");

    public static readonly DomainError ReasonTooLong =
        new("mileage_reading.reason_too_long", $"Le motif est limité à {MileageReading.MaxReasonLength} caractères.");

    /// <summary>Valeur inférieure au relevé précédent ou supérieure au suivant, sans motif.</summary>
    public static readonly DomainError ReasonRequired =
        new("mileage_reading.reason_required", "Le kilométrage est incohérent avec les relevés voisins : un motif est obligatoire.");

    /// <summary>Avertissement : levé en renvoyant la requête avec <c>confirmHighMileage: true</c>.</summary>
    public static readonly DomainError HighMileageConfirmationRequired =
        new(
            "mileage_reading.high_mileage_confirmation_required",
            $"Plus de {MileageConsistency.MaxKilometersPerDay} km/jour en moyenne depuis le relevé précédent : confirmation requise.");
}
