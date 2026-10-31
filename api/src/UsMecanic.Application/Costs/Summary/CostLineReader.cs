using UsMecanic.Application.Costs.ExpenseCategories;
using UsMecanic.Application.Costs.Expenses;
using UsMecanic.Application.Costs.Loans;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Summary;

/// <summary>
/// Lit les coûts de la période dans les trois sources (interventions, dépenses, emprunts) et les ramène à des lignes.
/// Les coûts des véhicules sortis du parc restent comptés (historique). Les dépenses futures (prévues) et les
/// mensualités des mois futurs sont incluses.
/// </summary>
public sealed class CostLineReader(
    IInterventionRepository interventions,
    IExpenseRepository expenses,
    IExpenseCategoryRepository categories,
    ILoanRepository loans)
{
    /// <summary>Catégorie sous laquelle apparaissent les interventions (famille ponctuelle).</summary>
    public const string MaintenanceCategory = "Entretien et réparation";

    /// <summary>Catégorie d'une dépense sans catégorie ou dont la catégorie est introuvable.</summary>
    public const string UncategorizedCategory = "Sans catégorie";

    /// <summary>Les emprunts n'ont pas de catégorie : leurs lignes portent ce libellé.</summary>
    public const string LoanCategory = "Emprunts";

    public async Task<IReadOnlyList<CostLine>> ReadAsync(DateOnly from, DateOnly to, Guid? vehicleId, CancellationToken cancellationToken)
    {
        var lines = new List<CostLine>();

        var done = await interventions.ListWithAmountInPeriodAsync(from, to, vehicleId, cancellationToken);
        lines.AddRange(done.Select(i => new CostLine(i.Date, CostFamily.OneOff, MaintenanceCategory, i.Amount!.Value)));

        var spent = await expenses.ListInPeriodAsync(from, to, vehicleId, cancellationToken);
        if (spent.Count > 0)
        {
            var names = (await categories.ListAsync(includeInactive: true, cancellationToken)).ToDictionary(c => c.Id, c => c.Name);
            lines.AddRange(spent.Select(e => new CostLine(e.Date, ToFamily(e.Family), CategoryName(e, names), e.Amount, e.CategoryId)));
        }

        // Une mensualité compte si son mois est l'un des mois touchés par la période.
        var firstMonth = CostAggregator.PeriodStart(from, CostGranularity.Month);
        var borrowed = await loans.ListStartedBeforeAsync(CostAggregator.PeriodStart(to, CostGranularity.Month).AddMonths(1), vehicleId, cancellationToken);
        lines.AddRange(borrowed
            .SelectMany(l => l.Installments())
            .Where(m => m.Month >= firstMonth && m.Month <= CostAggregator.PeriodStart(to, CostGranularity.Month))
            .Select(m => new CostLine(m.Month, CostFamily.Loan, LoanCategory, m.Amount)));

        return lines;
    }

    private static CostFamily ToFamily(ExpenseFamily family) =>
        family == ExpenseFamily.Running ? CostFamily.Running : CostFamily.OneOff;

    private static string CategoryName(Expense expense, Dictionary<Guid, string> names) =>
        expense.CategoryId is { } id && names.TryGetValue(id, out var name) ? name : UncategorizedCategory;
}
