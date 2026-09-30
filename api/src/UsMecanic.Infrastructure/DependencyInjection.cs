using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UsMecanic.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Enregistre les adaptateurs sortants (persistance, sources externes : télématique, saisie km…).
    /// Chaque source implémente un port défini dans la couche Application.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services;
    }
}
