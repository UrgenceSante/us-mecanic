namespace UsMecanic.Application.Costs.Summary;

/// <summary>Famille d'un coût dans la synthèse : les deux familles de dépenses, plus les mensualités d'emprunt.</summary>
public enum CostFamily
{
    OneOff,
    Running,
    Loan,
}
