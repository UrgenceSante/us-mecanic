namespace UsMecanic.Application.Vehicles;

public sealed class ListVehicles(IVehicleRepository repository, VehicleTypeLookup typeLookup)
{
    public async Task<IReadOnlyList<VehicleDto>> ExecuteAsync(VehicleFilter filter, CancellationToken cancellationToken)
    {
        var vehicles = await repository.ListAsync(filter, cancellationToken);
        return await typeLookup.ToDtosAsync(vehicles, cancellationToken);
    }
}
