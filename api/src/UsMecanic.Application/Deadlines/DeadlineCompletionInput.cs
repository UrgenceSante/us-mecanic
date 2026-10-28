namespace UsMecanic.Application.Deadlines;

/// <summary>Corps d'une requête « échéance réalisée ».</summary>
/// <param name="Date">Date réelle de réalisation (pas dans le futur).</param>
/// <param name="Kilometers">Km réel, facultatif ; s'il est fourni, un relevé kilométrique est créé.</param>
/// <param name="Comment">Commentaire facultatif (devient les travaux de l'intervention créée).</param>
/// <param name="Reason">Motif du relevé, obligatoire si le km est incohérent avec les relevés voisins.</param>
/// <param name="ConfirmHighMileage">À <c>true</c> pour confirmer un kilométrage moyen supérieur à 1 000 km/jour.</param>
/// <param name="GarageId">Garage actif de l'intervention créée, facultatif.</param>
public sealed record DeadlineCompletionInput(
    DateOnly Date,
    int? Kilometers = null,
    string? Comment = null,
    string? Reason = null,
    bool ConfirmHighMileage = false,
    Guid? GarageId = null)
{
    public const int MaxCommentLength = 1000;
}
