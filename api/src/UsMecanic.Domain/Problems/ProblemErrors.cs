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

    public static readonly DomainError NotFound =
        DomainError.NotFound("problem.not_found", "Problème introuvable.");

    public static readonly DomainError AlreadyAnalyzed =
        DomainError.Conflict("problem.already_analyzed", "Ce problème est déjà analysé.");

    public static readonly DomainError AlreadyResolved =
        DomainError.Conflict("problem.already_resolved", "Ce problème est résolu : il ne peut plus être analysé.");

    public static readonly DomainError AnalysisRequired =
        new("problem.analysis_required", "Le texte de l'analyse est obligatoire.");

    public static readonly DomainError AnalysisTooLong =
        new("problem.analysis_too_long", $"Le texte de l'analyse est limité à {Problem.MaxAnalysisLength} caractères.");

    public static readonly DomainError NatureUnknown =
        new("problem.nature_unknown", "La nature choisie n'existe pas.");

    public static readonly DomainError ElementUnknown =
        new("problem.element_unknown", "L'élément concerné choisi n'existe pas.");

    public static readonly DomainError DateInFuture =
        new("problem.date_in_future", "La date du signalement ne peut pas être dans le futur.");

    public static readonly DomainError NotAnalyzed =
        DomainError.Conflict("problem.not_analyzed", "Ce problème n'est pas encore analysé : on ne peut pas lui ajouter d'action.");

    public static readonly DomainError ActionNotFound =
        DomainError.NotFound("problem_action.not_found", "Action de résolution introuvable.");

    public static readonly DomainError ActionAlreadyClosed =
        DomainError.Conflict("problem_action.already_closed", "Cette action est déjà clôturée.");

    public static readonly DomainError ActionCommentTooLong =
        new("problem_action.comment_too_long", $"Le commentaire est limité à {ProblemAction.MaxCommentLength} caractères.");

    public static readonly DomainError ActorUnknown =
        new("problem_action.actor_unknown", "L'acteur choisi n'existe pas.");

    public static readonly DomainError ActionTypeUnknown =
        new("problem_action.type_unknown", "Le type d'action choisi n'existe pas.");

    public static readonly DomainError ConstraintUnknown =
        new("problem_action.constraint_unknown", "La contrainte choisie n'existe pas.");

    /// <summary>Intervention inconnue ou appartenant à un autre véhicule que celui du problème.</summary>
    public static readonly DomainError ActionInterventionInvalid =
        new("problem_action.intervention_invalid", "L'intervention liée doit exister dans le carnet d'entretien du véhicule du problème.");
}
