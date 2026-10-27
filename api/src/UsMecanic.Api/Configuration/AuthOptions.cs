namespace UsMecanic.Api.Configuration;

/// <summary>Configuration de l'authentification Keycloak (section "Auth").</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>URL du realm Keycloak, ex. https://auth.example.fr/realms/us.</summary>
    public string Authority { get; init; } = string.Empty;

    /// <summary>Audience attendue dans le token (client Keycloak).</summary>
    public string Audience { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;

    /// <summary>Nom de la clé de configuration (Auth:Disabled, variable Auth__Disabled) qui désactive l'authentification.</summary>
    public const string DisabledKey = "Auth:Disabled";

    /// <summary>
    /// Vrai uniquement si Auth:Disabled vaut explicitement "true" (ADR-0001, dev seulement).
    /// Absent, vide ou non booléen : l'authentification reste active (pas d'exception, pas de repli permissif).
    /// </summary>
    public static bool IsDisabled(IConfiguration configuration) =>
        bool.TryParse(configuration[DisabledKey], out var disabled) && disabled;
}
