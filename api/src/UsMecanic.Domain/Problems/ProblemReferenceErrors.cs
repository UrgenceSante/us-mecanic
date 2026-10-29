using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>Erreurs communes aux référentiels des problèmes ; les codes sont préfixés par le référentiel.</summary>
public static class ProblemReferenceErrors
{
    public static DomainError NotFound(ReferenceKind kind)
    {
        ArgumentNullException.ThrowIfNull(kind);

        return DomainError.NotFound($"{kind.Code}.not_found", $"{kind.Label} introuvable.");
    }

    public static DomainError Inactive(ReferenceKind kind)
    {
        ArgumentNullException.ThrowIfNull(kind);

        return new($"{kind.Code}.inactive", $"{kind.Label} : cette valeur est désactivée et ne peut plus être choisie.");
    }

    public static DomainError NameRequired(ReferenceKind kind)
    {
        ArgumentNullException.ThrowIfNull(kind);

        return new($"{kind.Code}.name_required", $"{kind.Label} : le libellé est obligatoire.");
    }

    public static DomainError NameTooLong(ReferenceKind kind)
    {
        ArgumentNullException.ThrowIfNull(kind);

        return new($"{kind.Code}.name_too_long", $"{kind.Label} : le libellé est limité à {ProblemReference.MaxNameLength} caractères.");
    }

    public static DomainError NameAlreadyExists(ReferenceKind kind)
    {
        ArgumentNullException.ThrowIfNull(kind);

        return DomainError.Conflict($"{kind.Code}.name_already_exists", $"{kind.Label} : ce libellé existe déjà.");
    }
}
