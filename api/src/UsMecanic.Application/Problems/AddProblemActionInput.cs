namespace UsMecanic.Application.Problems;

/// <summary>Corps d'une requête « ajouter une action de résolution ».</summary>
/// <param name="ActorId">Acteur (référentiel actif).</param>
/// <param name="TypeId">Type d'action (référentiel actif).</param>
/// <param name="ConstraintId">Contrainte (référentiel actif).</param>
/// <param name="DueDate">Échéance facultative.</param>
/// <param name="Comment">Commentaire facultatif.</param>
/// <param name="InterventionId">Intervention du carnet d'entretien du même véhicule, facultative.</param>
public sealed record AddProblemActionInput(Guid ActorId, Guid TypeId, Guid ConstraintId, DateOnly? DueDate, string? Comment, Guid? InterventionId);
