using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>
/// Supprime une intervention et ses liens avec les échéances. Les échéances ne sont pas recalculées (elles gardent
/// leur dernière réalisation) et le relevé kilométrique éventuellement créé par l'intervention est conservé.
/// Renvoie l'identifiant de l'intervention supprimée.
/// </summary>
public sealed class DeleteIntervention(MileageVehicleGuard guard, IInterventionRepository interventions)
{
    public async Task<Result<Guid>> ExecuteAsync(Guid vehicleId, Guid interventionId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<Guid>.Failure(error);
        }

        var intervention = await interventions.GetAsync(vehicleId, interventionId, cancellationToken);
        if (intervention is null)
        {
            return Result<Guid>.Failure(InterventionErrors.NotFound);
        }

        interventions.Remove(intervention);
        await interventions.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(intervention.Id);
    }
}
