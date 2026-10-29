namespace UsMecanic.Application.Problems;

/// <summary>Corps d'une requête « modifier une action » : les trois champs sont remplacés (valeur absente = champ vidé).</summary>
/// <param name="DueDate">Échéance facultative.</param>
/// <param name="Comment">Commentaire facultatif.</param>
/// <param name="InterventionId">Intervention du carnet d'entretien du même véhicule, facultative.</param>
public sealed record UpdateProblemActionInput(DateOnly? DueDate, string? Comment, Guid? InterventionId);
