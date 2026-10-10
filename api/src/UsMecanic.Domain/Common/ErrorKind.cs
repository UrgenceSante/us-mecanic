namespace UsMecanic.Domain.Common;

/// <summary>Nature d'une erreur métier, traduite en statut HTTP par la couche Api.</summary>
public enum ErrorKind
{
    /// <summary>Donnée invalide ou règle métier non respectée.</summary>
    Validation,

    /// <summary>Ressource introuvable.</summary>
    NotFound,

    /// <summary>Conflit avec l'état existant (doublon, transition interdite…).</summary>
    Conflict,
}
