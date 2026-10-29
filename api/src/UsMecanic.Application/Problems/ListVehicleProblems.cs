using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Problems;

/// <summary>Problèmes d'un véhicule, du plus récent au plus ancien (lecture autorisée pour un véhicule sorti du parc).</summary>
public sealed class ListVehicleProblems(MileageVehicleGuard guard, IProblemRepository problems)
{
    public async Task<Result<IReadOnlyList<ProblemDto>>> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<ProblemDto>>.Failure(error);
        }

        var list = await problems.ListByVehicleAsync(vehicleId, cancellationToken);
        return Result<IReadOnlyList<ProblemDto>>.Success([.. list.Select(ProblemDto.From)]);
    }
}
