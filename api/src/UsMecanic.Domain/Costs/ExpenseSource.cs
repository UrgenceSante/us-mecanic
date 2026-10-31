namespace UsMecanic.Domain.Costs;

/// <summary>Origine d'une dépense.</summary>
public enum ExpenseSource
{
    /// <summary>Saisie à la main : seule source modifiable et supprimable.</summary>
    Manual,

    /// <summary>Connecteur carte carburant (phase 8).</summary>
    Total,

    /// <summary>Connecteur télépéage (phase 8).</summary>
    Ulys,
}
