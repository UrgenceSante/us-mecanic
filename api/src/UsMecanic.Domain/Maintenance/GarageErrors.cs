using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Maintenance;

public static class GarageErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("garage.not_found", "Garage introuvable.");

    public static readonly DomainError NameRequired =
        new("garage.name_required", "Le nom du garage est obligatoire.");

    public static readonly DomainError NameTooLong =
        new("garage.name_too_long", $"Le nom du garage est limité à {Garage.MaxNameLength} caractères.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict("garage.name_already_exists", "Un garage porte déjà ce nom.");
}
