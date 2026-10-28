using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Application.Dashboard;
using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Maintenance.Garages;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Import;
using UsMecanic.Application.Vehicles.Types;

namespace UsMecanic.Application;

public static class DependencyInjection
{
    /// <summary>Enregistre les cas d'usage de la couche Application.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, string? businessTimeZoneId = null)
    {
        services.AddSingleton(TimeProvider.System);
        var zoneId = string.IsNullOrWhiteSpace(businessTimeZoneId) ? BusinessCalendar.DefaultTimeZoneId : businessTimeZoneId;
        var zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        services.AddSingleton(provider => new BusinessCalendar(provider.GetRequiredService<TimeProvider>(), zone));

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

        services.AddScoped<ListGarages>();
        services.AddScoped<CreateGarage>();
        services.AddScoped<RenameGarage>();
        services.AddScoped<DeactivateGarage>();

        services.AddScoped<InterventionDtoFactory>();
        services.AddScoped<ListInterventions>();
        services.AddScoped<AddIntervention>();

        services.AddScoped<ListDeadlineTypes>();
        services.AddScoped<CreateDeadlineType>();
        services.AddScoped<UpdateDeadlineType>();
        services.AddScoped<DeactivateDeadlineType>();
        services.AddScoped<GetDeadlineThresholds>();
        services.AddScoped<UpdateDeadlineThresholds>();
        services.AddScoped<MileageVehicleGuard>();
        services.AddScoped<ListMileageReadings>();
        services.AddScoped<AddMileageReading>();
        services.AddScoped<UpdateMileageReading>();
        services.AddScoped<DeleteMileageReading>();
        services.AddScoped<VehicleDeadlineDtoFactory>();
        services.AddScoped<ListVehicleDeadlines>();
        services.AddScoped<AddVehicleDeadline>();
        services.AddScoped<AssignDeadlineType>();
        services.AddScoped<UpdateVehicleDeadlineInterval>();
        services.AddScoped<CompleteVehicleDeadline>();
        services.AddScoped<ListDashboardDeadlines>();

        return services;
    }
}
