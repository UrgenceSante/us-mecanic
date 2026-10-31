using UsMecanic.Application.Costs.Summary;

namespace UsMecanic.UnitTests.Application.Costs;

public class CostAggregatorTests
{
    private static CostLine Line(string date, CostFamily family, string category, decimal amount) =>
        new(DateOnly.Parse(date, System.Globalization.CultureInfo.InvariantCulture), family, category, amount);

    private static CostSummaryDto Build(string from, string to, CostGranularity granularity, params CostLine[] lines) =>
        CostAggregator.Build(
            DateOnly.Parse(from, System.Globalization.CultureInfo.InvariantCulture),
            DateOnly.Parse(to, System.Globalization.CultureInfo.InvariantCulture),
            granularity,
            null,
            lines);

    [Fact]
    public void Renvoie_une_entree_par_mois_meme_vide()
    {
        var summary = Build("2026-01-15", "2026-03-02", CostGranularity.Month, Line("2026-02-10", CostFamily.OneOff, "Pneus", 100m));

        Assert.Equal(["2026-01", "2026-02", "2026-03"], summary.Periods.Select(p => p.Period));
        Assert.Equal([0m, 100m, 0m], summary.Periods.Select(p => p.Total));
        Assert.Equal(100m, summary.Total);
    }

    [Fact]
    public void Une_ligne_n_apparait_que_dans_son_mois()
    {
        var summary = Build("2026-01-01", "2026-12-31", CostGranularity.Month, Line("2026-03-15", CostFamily.OneOff, "Entretien et réparation", 120m));

        var march = Assert.Single(summary.Periods, p => p.Total != 0m);
        Assert.Equal("2026-03", march.Period);
        Assert.Equal(new DateOnly(2026, 3, 1), march.Start);
    }

    [Fact]
    public void Regroupe_par_annee()
    {
        var summary = Build(
            "2025-06-01",
            "2026-06-30",
            CostGranularity.Year,
            Line("2025-12-31", CostFamily.OneOff, "A", 10m),
            Line("2026-01-01", CostFamily.OneOff, "A", 20m),
            Line("2026-06-30", CostFamily.Running, "Carburant", 5m));

        Assert.Equal(["2025", "2026"], summary.Periods.Select(p => p.Period));
        Assert.Equal([10m, 25m], summary.Periods.Select(p => p.Total));
        Assert.Equal(35m, summary.Total);
    }

    [Fact]
    public void Les_trois_familles_sont_toujours_presentes_avec_leur_total()
    {
        var summary = Build(
            "2026-01-01",
            "2026-01-31",
            CostGranularity.Month,
            Line("2026-01-05", CostFamily.OneOff, "A", 10.10m),
            Line("2026-01-06", CostFamily.OneOff, "B", 0.20m),
            Line("2026-01-01", CostFamily.Loan, "Emprunts", 300m));

        Assert.Equal([CostFamily.OneOff, CostFamily.Running, CostFamily.Loan], summary.Families.Select(f => f.Family));
        Assert.Equal([10.30m, 0m, 300m], summary.Families.Select(f => f.Total));
        Assert.Equal(summary.Total, summary.Families.Sum(f => f.Total));
        Assert.Equal(summary.Total, summary.Periods.Sum(p => p.Total));
    }

    [Fact]
    public void Ventile_par_categorie_et_somme_les_lignes_d_une_meme_categorie()
    {
        var summary = Build(
            "2026-01-01",
            "2026-01-31",
            CostGranularity.Month,
            Line("2026-01-05", CostFamily.OneOff, "Pneus", 100m),
            Line("2026-01-20", CostFamily.OneOff, "Pneus", 50m),
            Line("2026-01-21", CostFamily.OneOff, "Lavage", 10m));

        var categories = summary.Periods[0].Categories;
        Assert.Equal(["Lavage", "Pneus"], categories.Select(c => c.Category));
        Assert.Equal([10m, 150m], categories.Select(c => c.Total));
        Assert.Equal(summary.Periods[0].Total, categories.Sum(c => c.Total));
    }

    [Fact]
    public void Ecarte_les_lignes_hors_de_la_periode()
    {
        var summary = Build(
            "2026-02-01",
            "2026-02-28",
            CostGranularity.Month,
            Line("2026-01-31", CostFamily.OneOff, "A", 10m),
            Line("2026-03-01", CostFamily.OneOff, "A", 20m));

        Assert.Equal(0m, summary.Total);
        Assert.Empty(summary.Categories);
    }

    [Fact]
    public void Une_periode_sans_cout_donne_des_totaux_nuls()
    {
        var summary = Build("2026-01-01", "2026-12-31", CostGranularity.Year);

        Assert.Equal(0m, summary.Total);
        Assert.All(summary.Families, f => Assert.Equal(0m, f.Total));
        Assert.Single(summary.Periods);
    }
}
