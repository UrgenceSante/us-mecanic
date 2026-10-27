namespace UsMecanic.Application.Deadlines;

/// <summary>Liste les types d'échéance. Les types désactivés ne sont plus proposés, sauf demande explicite.</summary>
public sealed class ListDeadlineTypes(IDeadlineTypeRepository repository)
{
    public async Task<IReadOnlyList<DeadlineTypeDto>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var types = await repository.ListAsync(includeInactive, cancellationToken);
        return [.. types.Select(DeadlineTypeDto.From)];
    }
}
