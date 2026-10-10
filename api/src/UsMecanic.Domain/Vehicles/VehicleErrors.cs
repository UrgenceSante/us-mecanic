using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Vehicles;

public static class VehicleErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("vehicle.not_found", "Véhicule introuvable.");

    public static readonly DomainError PlateAlreadyExists =
        DomainError.Conflict("vehicle.plate_already_exists", "Un véhicule avec cette immatriculation existe déjà.");

    public static readonly DomainError Retired =
        DomainError.Conflict("vehicle.retired", "Le véhicule est sorti du parc : il ne peut plus être modifié.");

    public static readonly DomainError FirstRegistrationInFuture =
        new("vehicle.first_registration_in_future", "La date de mise en circulation ne peut pas être dans le futur.");

    public static readonly DomainError FieldTooLong =
        new("vehicle.field_too_long", $"Les champs texte sont limités à {Vehicle.MaxTextLength} caractères.");
}
