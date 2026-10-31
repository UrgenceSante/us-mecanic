using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>
/// Port de persistance des interventions. L'ajout est enregistré avec le même <c>SaveChangesAsync</c> que les
/// échéances et les relevés (unité de travail partagée, donc une seule transaction).
/// </summary>
public interface IInterventionRepository
{
    /// <summary>Interventions d'un véhicule avec leurs échéances liées, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<Intervention>> ListByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);

    /// <summary>
    /// Interventions chiffrées (montant renseigné) datées dans la période (bornes incluses), pour un véhicule ou, sans
    /// filtre, pour tout le parc. Sans les échéances liées ; filtrées côté base.
    /// </summary>
    Task<IReadOnlyList<Intervention>> ListWithAmountInPeriodAsync(DateOnly from, DateOnly to, Guid? vehicleId, CancellationToken cancellationToken);

    /// <summary>Intervention du véhicule avec ses échéances liées, suivie par le contexte (pour la supprimer).</summary>
    Task<Intervention?> GetAsync(Guid vehicleId, Guid interventionId, CancellationToken cancellationToken);

    void Add(Intervention intervention);

    /// <summary>Supprime l'intervention et ses liens avec les échéances (les échéances et les relevés ne sont pas touchés).</summary>
    void Remove(Intervention intervention);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
