using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Loans;

/// <summary>Corps d'une requête de création ou de modification d'un emprunt.</summary>
/// <param name="Lender">Organisme prêteur.</param>
/// <param name="BorrowedAmount">Montant emprunté, en euros (2 décimales, zéro autorisé).</param>
/// <param name="MonthlyPayment">Mensualité, en euros (2 décimales, zéro autorisé).</param>
/// <param name="StartDate">Date de début de l'emprunt.</param>
/// <param name="DurationMonths">Durée en mois (entier strictement positif), modifiable.</param>
public sealed record LoanInput(string? Lender, decimal? BorrowedAmount, decimal? MonthlyPayment, DateOnly StartDate, int DurationMonths)
{
    public LoanEntry ToEntry() => new(Lender, BorrowedAmount, MonthlyPayment, StartDate, DurationMonths);
}
