namespace UsMecanic.Domain.Problems;

/// <summary>Avancement du traitement d'un problème.</summary>
public enum ProblemStatus
{
    /// <summary>Signalé par un chauffeur, pas encore analysé.</summary>
    Reported,

    /// <summary>Analysé par l'administrateur.</summary>
    Analyzed,

    /// <summary>Toutes les actions de résolution sont closes.</summary>
    Resolved,
}
