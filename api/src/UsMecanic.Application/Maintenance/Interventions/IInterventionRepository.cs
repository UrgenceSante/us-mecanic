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

    void Add(Intervention intervention);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
