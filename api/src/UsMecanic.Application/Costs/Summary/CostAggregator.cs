namespace UsMecanic.Application.Costs.Summary;

/// <summary>
/// Règle d'agrégation, sans accès aux données : range les coûts par période, famille et catégorie.
/// Toutes les périodes entre <c>from</c> et <c>to</c> sont renvoyées, même vides.
/// </summary>
public static class CostAggregator
{
    private static readonly StringComparer CategoryComparer = StringComparer.InvariantCultureIgnoreCase;

    public static CostSummaryDto Build(DateOnly from, DateOnly to, CostGranularity granularity, Guid? vehicleId, IEnumerable<CostLine> lines, CostPerKmDto costPerKm)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(costPerKm);

        var first = PeriodStart(from, granularity);
        var last = PeriodStart(to, granularity);
        var byPeriod = lines
            .Select(l => (Start: PeriodStart(l.Date, granularity), Line: l))
            .Where(x => x.Start >= first && x.Start <= last)
            .ToLookup(x => x.Start, x => x.Line);

        var periods = new List<CostPeriodDto>();
        for (var start = first; start <= last; start = Next(start, granularity))
        {
            periods.Add(BuildPeriod(start, granularity, [.. byPeriod[start]]));
        }

        List<CostLine> all = [.. byPeriod.SelectMany(g => g)];
        return new CostSummaryDto(from, to, granularity, vehicleId, Sum(all), FamilyTotals(all), CategoryTotals(all), periods, costPerKm);
    }

    /// <summary>Premier jour de la période qui contient la date.</summary>
    public static DateOnly PeriodStart(DateOnly date, CostGranularity granularity) =>
        granularity == CostGranularity.Year ? new DateOnly(date.Year, 1, 1) : new DateOnly(date.Year, date.Month, 1);

    private static DateOnly Next(DateOnly start, CostGranularity granularity) =>
        granularity == CostGranularity.Year ? start.AddYears(1) : start.AddMonths(1);

    private static CostPeriodDto BuildPeriod(DateOnly start, CostGranularity granularity, List<CostLine> lines)
    {
        var label = granularity == CostGranularity.Year ? $"{start.Year:0000}" : $"{start.Year:0000}-{start.Month:00}";
        return new CostPeriodDto(label, start, Sum(lines), FamilyTotals(lines), CategoryTotals(lines));
    }

    private static decimal Sum(IEnumerable<CostLine> lines) => decimal.Round(lines.Sum(l => l.Amount), 2);

    private static List<CostFamilyTotal> FamilyTotals(List<CostLine> lines) =>
        [.. Enum.GetValues<CostFamily>().Select(f => new CostFamilyTotal(f, Sum(lines.Where(l => l.Family == f))))];

    private static List<CostCategoryTotal> CategoryTotals(List<CostLine> lines) =>
        [
            .. lines
                .GroupBy(l => (l.Family, l.CategoryId, l.Category))
                .OrderBy(g => g.Key.Family)
                .ThenBy(g => g.Key.Category, CategoryComparer)
                .Select(g => new CostCategoryTotal(g.Key.Family, g.Key.CategoryId, g.Key.Category, Sum(g))),
        ];
}
