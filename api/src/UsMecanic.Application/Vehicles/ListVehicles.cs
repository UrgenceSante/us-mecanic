namespace UsMecanic.Application.Vehicles;

public sealed class ListVehicles(IVehicleRepository repository, VehicleTypeLookup typeLookup, VehicleAlertsLookup alerts)
{
    public async Task<IReadOnlyList<VehicleDto>> ExecuteAsync(VehicleFilter filter, CancellationToken cancellationToken)
    {
        var vehicles = await repository.ListAsync(filter, cancellationToken);
        var dtos = await typeLookup.ToDtosAsync(vehicles, cancellationToken);

        // L'utilisateur actuel (R4) est déjà calculé pour toute la liste dans les DTO : filtre sans requête de plus.
        if (filter.CurrentUserCompanyId is { } userCompanyId)
        {
            dtos = [.. dtos.Where(d => d.CurrentUser.Company?.Id == userCompanyId)];
        }

        return await alerts.WithAlertsAsync(dtos, cancellationToken);
    }
}
