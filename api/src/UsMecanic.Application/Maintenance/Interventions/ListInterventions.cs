using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>Carnet d'entretien d'un véhicule : interventions de la plus récente à la plus ancienne.</summary>
public sealed class ListInterventions(MileageVehicleGuard guard, IInterventionRepository interventions, InterventionDtoFactory factory)
{
    public async Task<Result<IReadOnlyList<InterventionDto>>> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<InterventionDto>>.Failure(error);
        }

        var list = await interventions.ListByVehicleAsync(vehicleId, cancellationToken);
        return Result<IReadOnlyList<InterventionDto>>.Success(await factory.BuildAsync(vehicleId, list, cancellationToken));
    }
}
