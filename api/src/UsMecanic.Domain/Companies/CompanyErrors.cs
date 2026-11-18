using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Companies;

public static class CompanyErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("company.not_found", "Société introuvable.");

    public static readonly DomainError NameRequired =
        new("company.name_required", "Le nom de la société est obligatoire.");

    public static readonly DomainError NameTooLong =
        new("company.name_too_long", $"Le nom de la société est limité à {Company.MaxNameLength} caractères.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict("company.name_already_exists", "Une société porte déjà ce nom.");

    public static readonly DomainError Unknown =
        new("company.unknown", "Société inconnue.");

    public static readonly DomainError Inactive =
        new("company.inactive", "Cette société est désactivée : elle ne peut plus être choisie comme propriétaire ou locataire.");
}
