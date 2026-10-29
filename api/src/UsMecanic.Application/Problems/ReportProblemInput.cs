namespace UsMecanic.Application.Problems;

/// <summary>Corps d'une requête « signaler un problème ».</summary>
/// <param name="LicensePlate">Immatriculation du véhicule (normalisée comme à l'enregistrement d'un véhicule).</param>
/// <param name="Date">Date du signalement (pas dans le futur).</param>
/// <param name="Description">Description du problème (obligatoire).</param>
/// <param name="Author">Auteur du signalement : identifiant texte, nom ou matricule (obligatoire).</param>
public sealed record ReportProblemInput(string? LicensePlate, DateOnly Date, string? Description = null, string? Author = null);
