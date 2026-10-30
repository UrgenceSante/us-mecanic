using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>
/// Modifie (ou vide) le montant d'une intervention, avec les validations de la création. Autorisé pour un véhicule
/// sorti du parc : corriger un coût n'est pas une saisie d'activité (même choix que pour les emprunts).
/// </summary>
public sealed class ChangeInterventionAmount(MileageVehicleGuard guard, IInterventionRepository interventions, InterventionDtoFactory factory)
{
    public async Task<Result<InterventionDto>> ExecuteAsync(
        Guid vehicleId, Guid interventionId, decimal? amount, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<InterventionDto>.Failure(error);
        }

        var intervention = await interventions.GetAsync(vehicleId, interventionId, cancellationToken);
        if (intervention is null)
        {
            return Result<InterventionDto>.Failure(InterventionErrors.NotFound);
        }

        var changed = intervention.ChangeAmount(amount);
        if (changed.IsFailure)
        {
            return Result<InterventionDto>.Failure(changed.Error!);
        }

        await interventions.SaveChangesAsync(cancellationToken);
        return Result<InterventionDto>.Success((await factory.BuildAsync(vehicleId, [intervention], cancellationToken))[0]);
    }
}
