using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Vehicles;

public static class VehicleTypeErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("vehicle_type.not_found", "Type de véhicule introuvable.");

    public static readonly DomainError NameRequired =
        new("vehicle_type.name_required", "Le nom du type de véhicule est obligatoire.");

    public static readonly DomainError NameTooLong =
        new("vehicle_type.name_too_long", $"Le nom du type de véhicule est limité à {VehicleType.MaxNameLength} caractères.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict("vehicle_type.name_already_exists", "Un type de véhicule porte déjà ce nom.");

    public static readonly DomainError Unknown =
        new("vehicle_type.unknown", "Le type de véhicule indiqué n'existe pas.");

    public static readonly DomainError Inactive =
        new("vehicle_type.inactive", "Ce type de véhicule est désactivé : il ne peut plus être attribué.");
}
