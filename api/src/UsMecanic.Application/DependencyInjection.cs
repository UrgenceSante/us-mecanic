using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Import;

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

        return services;
    }
}
