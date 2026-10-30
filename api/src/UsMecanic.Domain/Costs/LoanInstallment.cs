namespace UsMecanic.Domain.Costs;

/// <summary>Mensualité d'un emprunt : le mois d'échéance (premier jour du mois) et son montant en euros.</summary>
public sealed record LoanInstallment(DateOnly Month, decimal Amount);
