namespace UsMecanic.Application.Vehicles;

public sealed class ListVehicles(IVehicleRepository repository)
{
    public async Task<IReadOnlyList<VehicleDto>> ExecuteAsync(VehicleFilter filter, CancellationToken cancellationToken)
    {
        var vehicles = await repository.ListAsync(filter, cancellationToken);
        return [.. vehicles.Select(VehicleDto.From)];
    }
}
