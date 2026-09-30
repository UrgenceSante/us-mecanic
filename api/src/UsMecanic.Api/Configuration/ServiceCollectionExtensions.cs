using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace UsMecanic.Api.Configuration;

internal static class ServiceCollectionExtensions
{
    public const string CorsPolicy = "front";

    /// <summary>
    /// Authentification JWT (Keycloak). Par défaut, tout endpoint exige un utilisateur authentifié :
    /// les endpoints publics doivent l'indiquer explicitement avec AllowAnonymous().
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var auth = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = auth.Authority;
                options.Audience = auth.Audience;
                options.RequireHttpsMetadata = auth.RequireHttpsMetadata;
                options.TokenValidationParameters.ValidateAudience = !string.IsNullOrEmpty(auth.Audience);
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }

    public static IServiceCollection AddFrontCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        return services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
    }
}
