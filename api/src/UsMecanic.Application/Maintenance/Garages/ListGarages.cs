namespace UsMecanic.Application.Maintenance.Garages;

/// <summary>Liste les garages. Les garages désactivés ne sont plus proposés, sauf demande explicite.</summary>
public sealed class ListGarages(IGarageRepository repository)
{
    public async Task<IReadOnlyList<GarageDto>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var garages = await repository.ListAsync(includeInactive, cancellationToken);
        return [.. garages.Select(GarageDto.From)];
    }
}
