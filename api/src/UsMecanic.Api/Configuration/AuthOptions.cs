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
}
