using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

public static class ProblemErrors
{
    /// <summary>Aucun véhicule du parc ne porte l'immatriculation du signalement.</summary>
    public static readonly DomainError VehicleUnknown =
        DomainError.NotFound("problem.vehicle_unknown", "Aucun véhicule ne correspond à cette immatriculation.");

    public static readonly DomainError DescriptionRequired =
        new("problem.description_required", "La description du problème est obligatoire.");

    public static readonly DomainError DescriptionTooLong =
        new("problem.description_too_long", $"La description est limitée à {Problem.MaxDescriptionLength} caractères.");

    public static readonly DomainError AuthorRequired =
        new("problem.author_required", "L'auteur du signalement est obligatoire.");

    public static readonly DomainError AuthorTooLong =
        new("problem.author_too_long", $"L'auteur est limité à {Problem.MaxAuthorLength} caractères.");

    public static readonly DomainError DateInFuture =
        new("problem.date_in_future", "La date du signalement ne peut pas être dans le futur.");
}
