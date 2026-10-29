namespace UsMecanic.Domain.Problems;

/// <summary>Niveau de blocage d'un problème, du plus au moins bloquant (l'ordre des valeurs sert au tri).</summary>
public enum ProblemBlockingLevel
{
    /// <summary>Problème immobilisant non résolu.</summary>
    Immobilizing,

    /// <summary>Problème signalé, pas encore analysé.</summary>
    Unanalyzed,

    /// <summary>Au moins une action non close dont l'échéance est passée.</summary>
    OverdueAction,

    /// <summary>Tous les autres cas.</summary>
    Other,
}
