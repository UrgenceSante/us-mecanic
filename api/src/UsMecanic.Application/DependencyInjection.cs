using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Import;
using UsMecanic.Application.Vehicles.Types;

namespace UsMecanic.Application;

public static class DependencyInjection
{
    /// <summary>Enregistre les cas d'usage de la couche Application.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<RegisterVehicle>();
        services.AddScoped<UpdateVehicleDetails>();
        services.AddScoped<ChangeVehicleStatus>();
        services.AddScoped<GetVehicle>();
        services.AddScoped<ListVehicles>();
        services.AddScoped<ImportVehicles>();
        services.AddScoped<VehicleTypeLookup>();

        services.AddScoped<ListVehicleTypes>();
        services.AddScoped<CreateVehicleType>();
        services.AddScoped<RenameVehicleType>();
        services.AddScoped<DeactivateVehicleType>();

        services.AddScoped<ListDeadlineTypes>();
        services.AddScoped<CreateDeadlineType>();
        services.AddScoped<UpdateDeadlineType>();
        services.AddScoped<DeactivateDeadlineType>();
        services.AddScoped<GetDeadlineThresholds>();
        services.AddScoped<UpdateDeadlineThresholds>();

        return services;
    }
}
