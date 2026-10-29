using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Maintenance.Garages;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Application.Problems;
using UsMecanic.Application.Problems.References;
using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Import;
using UsMecanic.Application.Vehicles.Types;
using UsMecanic.Infrastructure.Deadlines;
using UsMecanic.Infrastructure.Maintenance;
using UsMecanic.Infrastructure.Mileage;
using UsMecanic.Infrastructure.Persistence;
using UsMecanic.Infrastructure.Problems;
using UsMecanic.Infrastructure.Vehicles;

namespace UsMecanic.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    /// <summary>
    /// Enregistre les adaptateurs sortants (persistance, sources externes : télématique, saisie km…).
    /// Chaque source implémente un port défini dans la couche Application.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Chaîne de connexion 'ConnectionStrings:{ConnectionStringName}' manquante.");
        }

        services.AddDbContext<UsMecanicDbContext>(options => ConfigureDbContext(options, connectionString));
        services.AddHealthChecks().AddDbContextCheck<UsMecanicDbContext>("database");

        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
        services.AddScoped<IGarageRepository, GarageRepository>();
        services.AddScoped(typeof(IProblemReferenceRepository<>), typeof(ProblemReferenceRepository<>));
        services.AddScoped<IInterventionRepository, InterventionRepository>();
        services.AddScoped<IProblemRepository, ProblemRepository>();
        services.AddScoped<IDeadlineTypeRepository, DeadlineTypeRepository>();
        services.AddScoped<IDeadlineThresholdsRepository, DeadlineThresholdsRepository>();
        services.AddScoped<IMileageReadingRepository, MileageReadingRepository>();
        services.AddScoped<IVehicleDeadlineRepository, VehicleDeadlineRepository>();
        services.AddSingleton<IVehicleImportReader, CsvVehicleImportReader>();

        return services;
    }

    internal static void ConfigureDbContext(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
}
