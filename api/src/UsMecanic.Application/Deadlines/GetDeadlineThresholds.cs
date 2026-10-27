namespace UsMecanic.Application.Deadlines;

/// <summary>Lit les seuils globaux « échéance proche ».</summary>
public sealed class GetDeadlineThresholds(IDeadlineThresholdsRepository repository)
{
    public async Task<DeadlineThresholdsDto> ExecuteAsync(CancellationToken cancellationToken) =>
        DeadlineThresholdsDto.From(await repository.GetAsync(cancellationToken));
}
