using UsMecanic.Domain.Common;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Garages;

/// <summary>
/// Désactive un garage : il n'est plus proposé mais reste affiché sur les interventions qui l'ont.
/// Il n'existe volontairement aucune suppression de garage.
/// </summary>
public sealed class DeactivateGarage(IGarageRepository repository)
{
    public async Task<Result<GarageDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var garage = await repository.GetByIdAsync(id, cancellationToken);
        if (garage is null)
        {
            return Result<GarageDto>.Failure(GarageErrors.NotFound);
        }

        garage.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return Result<GarageDto>.Success(GarageDto.From(garage));
    }
}
