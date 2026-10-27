namespace UsMecanic.Application.Vehicles.Types;

/// <summary>Liste les types de véhicule. Les types désactivés ne sont plus proposés, sauf demande explicite.</summary>
public sealed class ListVehicleTypes(IVehicleTypeRepository repository)
{
    public async Task<IReadOnlyList<VehicleTypeDto>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var types = await repository.ListAsync(includeInactive, cancellationToken);
        return [.. types.Select(VehicleTypeDto.From)];
    }
}
