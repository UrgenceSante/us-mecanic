namespace UsMecanic.Application.Vehicles;

public sealed class ListVehicles(IVehicleRepository repository, VehicleTypeLookup typeLookup, VehicleAlertsLookup alerts)
{
    public async Task<IReadOnlyList<VehicleDto>> ExecuteAsync(VehicleFilter filter, CancellationToken cancellationToken)
    {
        var vehicles = await repository.ListAsync(filter, cancellationToken);
        var dtos = await typeLookup.ToDtosAsync(vehicles, cancellationToken);
        return await alerts.WithAlertsAsync(dtos, cancellationToken);
    }
}
