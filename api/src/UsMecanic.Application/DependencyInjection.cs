using Microsoft.Extensions.DependencyInjection;

namespace UsMecanic.Application;

public static class DependencyInjection
{
    /// <summary>Enregistre les cas d'usage de la couche Application.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
