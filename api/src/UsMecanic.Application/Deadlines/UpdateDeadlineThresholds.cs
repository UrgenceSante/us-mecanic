using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Deadlines;

/// <summary>Modifie les seuils globaux « échéance proche » (jours et kilomètres, strictement positifs).</summary>
public sealed class UpdateDeadlineThresholds(IDeadlineThresholdsRepository repository)
{
    public async Task<Result<DeadlineThresholdsDto>> ExecuteAsync(int? dueSoonDays, int? dueSoonKm, CancellationToken cancellationToken)
    {
        var thresholds = await repository.GetAsync(cancellationToken);
        var updated = thresholds.Update(dueSoonDays, dueSoonKm);
        if (updated.IsFailure)
        {
            return Result<DeadlineThresholdsDto>.Failure(updated.Error!);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result<DeadlineThresholdsDto>.Success(DeadlineThresholdsDto.From(thresholds));
    }
}
