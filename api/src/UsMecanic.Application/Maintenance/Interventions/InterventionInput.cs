namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>Corps d'une requête « ajouter une intervention ».</summary>
/// <param name="Date">Date réelle de l'intervention (pas dans le futur).</param>
/// <param name="Kilometers">Km réel, facultatif ; s'il est fourni, un relevé kilométrique est créé.</param>
/// <param name="GarageId">Garage actif, facultatif.</param>
/// <param name="Work">Travaux réalisés (obligatoire).</param>
/// <param name="DeadlineIds">Échéances en cours du véhicule réalisées par l'intervention (aucune = imprévue).</param>
/// <param name="Reason">Motif du relevé, obligatoire si le km est incohérent avec les relevés voisins.</param>
/// <param name="ConfirmHighMileage">À <c>true</c> pour confirmer un kilométrage moyen supérieur à 1 000 km/jour.</param>
public sealed record InterventionInput(
    DateOnly Date,
    int? Kilometers = null,
    Guid? GarageId = null,
    string? Work = null,
    IReadOnlyList<Guid>? DeadlineIds = null,
    string? Reason = null,
    bool ConfirmHighMileage = false);
