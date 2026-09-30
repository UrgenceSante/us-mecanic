using System.Reflection;
using System.Security.Claims;

namespace UsMecanic.Api.Endpoints;

internal static class SystemEndpoints
{
    public sealed record AppInfo(string Name, string Version, string Environment);

    public sealed record CurrentUser(string? Id, string? UserName);

    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health").AllowAnonymous();

        var api = app.MapGroup("/api").WithTags("System");

        api.MapGet("/info", (IHostEnvironment env) => new AppInfo("us-mecanic-api", GetVersion(), env.EnvironmentName))
            .AllowAnonymous()
            .WithSummary("Version et environnement de l'API");

        api.MapGet("/me", (ClaimsPrincipal user) => new CurrentUser(
                user.FindFirstValue(ClaimTypes.NameIdentifier),
                user.FindFirstValue("preferred_username")))
            .WithSummary("Utilisateur authentifié");

        return app;
    }

    private static string GetVersion() =>
        typeof(SystemEndpoints).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            .Split('+')[0] ?? "0.0.0";
}
