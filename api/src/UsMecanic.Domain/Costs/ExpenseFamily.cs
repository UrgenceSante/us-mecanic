namespace UsMecanic.Domain.Costs;

/// <summary>Famille d'une dépense.</summary>
public enum ExpenseFamily
{
    /// <summary>Dépense ponctuelle hors carnet d'entretien : seule famille saisissable à la main.</summary>
    OneOff,

    /// <summary>Utilisation courante (carburant, péage) : alimentée par les connecteurs de la phase 8.</summary>
    Running,
}
