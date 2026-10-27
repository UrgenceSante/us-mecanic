namespace UsMecanic.Api.Configuration;

/// <summary>Messages de journal liés à l'authentification (aucune donnée personnelle).</summary>
internal static partial class AuthLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Authentification DESACTIVEE (Auth:Disabled=true) : tous les endpoints sont ouverts. Réservé au développement.")]
    public static partial void AuthenticationDisabled(ILogger logger);
}
