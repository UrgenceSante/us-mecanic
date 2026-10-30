namespace UsMecanic.Domain.Costs;

/// <summary>Valeurs saisies pour un emprunt, avant validation.</summary>
/// <param name="Lender">Organisme prêteur.</param>
/// <param name="BorrowedAmount">Montant emprunté, en euros (obligatoire, 2 décimales, zéro autorisé).</param>
/// <param name="MonthlyPayment">Mensualité, en euros (obligatoire, 2 décimales, zéro autorisé).</param>
/// <param name="StartDate">Date de début : donne le mois de la première mensualité.</param>
/// <param name="DurationMonths">Durée en mois, entière et strictement positive.</param>
public sealed record LoanEntry(string? Lender, decimal? BorrowedAmount, decimal? MonthlyPayment, DateOnly StartDate, int DurationMonths);
