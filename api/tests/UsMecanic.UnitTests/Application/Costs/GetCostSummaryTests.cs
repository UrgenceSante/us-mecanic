using UsMecanic.Application.Costs.Summary;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Costs;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Costs;

public class GetCostSummaryTests
{
    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryInterventionRepository _interventions = new();
    private readonly InMemoryExpenseRepository _expenses = new();
    private readonly InMemoryExpenseCategoryRepository _categories = new();
    private readonly InMemoryLoanRepository _loans = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;

    public GetCostSummaryTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
    }

    private GetCostSummary UseCase => new(new MileageVehicleGuard(_vehicles), new CostLineReader(_interventions, _expenses, _categories, _loans));

    private static CostSummaryQuery Query(string from, string to, string? granularity = "month", Guid? vehicleId = null) =>
        new(
            DateOnly.Parse(from, System.Globalization.CultureInfo.InvariantCulture),
            DateOnly.Parse(to, System.Globalization.CultureInfo.InvariantCulture),
            granularity,
            vehicleId);

    private static DateOnly Day(string date) => DateOnly.Parse(date, System.Globalization.CultureInfo.InvariantCulture);

    private void AddIntervention(Guid vehicleId, string date, decimal? amount) =>
        _interventions.Saved.Add(Intervention.Create(vehicleId, Day(date), null, null, "Vidange", [], FixedClock.DefaultToday, _clock.Now, amount).Value);

    private void AddExpense(Guid vehicleId, string date, Guid categoryId, decimal amount) =>
        _expenses.Saved.Add(Expense.CreateManual(vehicleId, ExpenseFamily.OneOff, Day(date), categoryId, amount, null, _clock.Now).Value);

    private void AddLoan(Guid vehicleId, string start, int months, decimal monthly) =>
        _loans.Saved.Add(Loan.Create(vehicleId, new LoanEntry("Banque", 10_000m, monthly, Day(start), months), _clock.Now).Value);

    [Fact]
    public async Task Une_intervention_de_120_euros_du_15_mars_n_apparait_qu_en_mars()
    {
        AddIntervention(_vehicle.Id, "2026-03-15", 120m);

        var result = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-12-31"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var filled = Assert.Single(result.Value.Periods, p => p.Total != 0m);
        Assert.Equal("2026-03", filled.Period);
        Assert.Equal(120m, filled.Total);
        var category = Assert.Single(filled.Categories);
        Assert.Equal(CostFamily.OneOff, category.Family);
        Assert.Equal("Entretien et réparation", category.Category);
    }

    [Fact]
    public async Task Ignore_les_interventions_sans_montant()
    {
        AddIntervention(_vehicle.Id, "2026-03-15", null);
        AddIntervention(_vehicle.Id, "2026-03-16", 0m);

        var result = await UseCase.ExecuteAsync(Query("2026-03-01", "2026-03-31"), CancellationToken.None);

        Assert.Equal(0m, result.Value.Total);
        Assert.DoesNotContain(result.Value.Categories, c => c.Total != 0m);
    }

    [Fact]
    public async Task Un_emprunt_de_300_euros_sur_12_mois_donne_12_mensualites_mois_futurs_compris()
    {
        AddLoan(_vehicle.Id, "2026-01-10", 12, 300m);

        var result = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-12-31"), CancellationToken.None);

        Assert.Equal(12, result.Value.Periods.Count(p => p.Total == 300m));
        Assert.Equal(3600m, result.Value.Families.Single(f => f.Family == CostFamily.Loan).Total);
        Assert.Equal(3600m, result.Value.Total);
    }

    [Fact]
    public async Task Les_mensualites_a_cheval_sur_deux_annees_sont_reparties_par_annee()
    {
        AddLoan(_vehicle.Id, "2025-11-01", 6, 200m);

        var result = await UseCase.ExecuteAsync(Query("2025-01-01", "2026-12-31", "year"), CancellationToken.None);

        Assert.Equal(["2025", "2026"], result.Value.Periods.Select(p => p.Period));
        Assert.Equal([400m, 800m], result.Value.Periods.Select(p => p.Total));
    }

    [Fact]
    public async Task Ne_compte_que_les_mensualites_des_mois_de_la_periode()
    {
        AddLoan(_vehicle.Id, "2026-01-01", 12, 300m);

        var result = await UseCase.ExecuteAsync(Query("2026-03-20", "2026-04-05"), CancellationToken.None);

        Assert.Equal(600m, result.Value.Total);
    }

    [Fact]
    public async Task La_mensualite_du_mois_de_fin_compte_meme_si_l_emprunt_debute_apres_la_date_de_fin()
    {
        AddLoan(_vehicle.Id, "2026-04-15", 12, 300m);

        var result = await UseCase.ExecuteAsync(Query("2026-04-01", "2026-04-10"), CancellationToken.None);

        Assert.Equal(300m, result.Value.Total);
    }

    [Fact]
    public async Task Une_categorie_utilisateur_homonyme_des_interventions_reste_distincte()
    {
        var homonym = _categories.Seed("Entretien et réparation");
        AddExpense(_vehicle.Id, "2026-03-01", homonym.Id, 10m);
        AddIntervention(_vehicle.Id, "2026-03-02", 20m);

        var result = await UseCase.ExecuteAsync(Query("2026-03-01", "2026-03-31"), CancellationToken.None);

        Assert.Equal(2, result.Value.Categories.Count);
        Assert.Contains(result.Value.Categories, c => c.CategoryId == homonym.Id && c.Total == 10m);
        Assert.Contains(result.Value.Categories, c => c.CategoryId is null && c.Total == 20m);
    }

    [Fact]
    public async Task Un_emprunt_commencant_apres_la_periode_ou_termine_avant_ne_compte_pas()
    {
        AddLoan(_vehicle.Id, "2027-01-01", 12, 300m);
        AddLoan(_vehicle.Id, "2024-01-01", 12, 300m);

        var result = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-12-31"), CancellationToken.None);

        Assert.Equal(0m, result.Value.Total);
    }

    [Fact]
    public async Task Regroupe_les_depenses_par_categorie_et_inclut_les_depenses_prevues()
    {
        var tyres = _categories.Seed("Pneus");
        AddExpense(_vehicle.Id, "2026-02-01", tyres.Id, 80m);
        AddExpense(_vehicle.Id, "2027-02-01", tyres.Id, 20m);

        var result = await UseCase.ExecuteAsync(Query("2026-01-01", "2027-12-31", "year"), CancellationToken.None);

        Assert.Equal([80m, 20m], result.Value.Periods.Select(p => p.Total));
        var total = Assert.Single(result.Value.Categories);
        Assert.Equal("Pneus", total.Category);
        Assert.Equal(100m, total.Total);
    }

    [Fact]
    public async Task Le_filtre_vehicule_exclut_les_autres_et_le_parc_les_additionne()
    {
        var other = _vehicles.Seed("EF-456-GH");
        AddIntervention(_vehicle.Id, "2026-03-15", 100m);
        AddIntervention(other.Id, "2026-03-16", 40m);

        var one = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-12-31", vehicleId: _vehicle.Id), CancellationToken.None);
        var fleet = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-12-31"), CancellationToken.None);

        Assert.Equal(100m, one.Value.Total);
        Assert.Equal(_vehicle.Id, one.Value.VehicleId);
        Assert.Equal(140m, fleet.Value.Total);
    }

    [Fact]
    public async Task Les_couts_d_un_vehicule_sorti_du_parc_restent_comptes()
    {
        AddIntervention(_vehicle.Id, "2026-03-15", 100m);
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var one = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-12-31", vehicleId: _vehicle.Id), CancellationToken.None);
        var fleet = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-12-31"), CancellationToken.None);

        Assert.Equal(100m, one.Value.Total);
        Assert.Equal(100m, fleet.Value.Total);
    }

    [Fact]
    public async Task Un_vehicule_sans_depense_donne_des_totaux_nuls()
    {
        var result = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-03-31", vehicleId: _vehicle.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Total);
        Assert.Equal(3, result.Value.Periods.Count);
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu()
    {
        var result = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-03-31", vehicleId: Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task La_granularite_est_mensuelle_par_defaut_et_insensible_a_la_casse()
    {
        var byDefault = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-02-28", granularity: null), CancellationToken.None);
        var upper = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-02-28", granularity: "YEAR"), CancellationToken.None);

        Assert.Equal(CostGranularity.Month, byDefault.Value.Granularity);
        Assert.Equal(CostGranularity.Year, upper.Value.Granularity);
    }

    [Fact]
    public async Task Valide_la_requete()
    {
        var noFrom = await UseCase.ExecuteAsync(new CostSummaryQuery(null, Day("2026-01-01"), "month", null), CancellationToken.None);
        var noTo = await UseCase.ExecuteAsync(new CostSummaryQuery(Day("2026-01-01"), null, "month", null), CancellationToken.None);
        var reversed = await UseCase.ExecuteAsync(Query("2026-02-01", "2026-01-01"), CancellationToken.None);
        var unknown = await UseCase.ExecuteAsync(Query("2026-01-01", "2026-02-01", "week"), CancellationToken.None);
        var tooLong = await UseCase.ExecuteAsync(Query("2010-01-01", "2026-02-01"), CancellationToken.None);

        Assert.Equal(CostSummaryErrors.FromRequired, noFrom.Error);
        Assert.Equal(CostSummaryErrors.ToRequired, noTo.Error);
        Assert.Equal(CostSummaryErrors.InvalidRange, reversed.Error);
        Assert.Equal(CostSummaryErrors.InvalidGranularity, unknown.Error);
        Assert.Equal(CostSummaryErrors.RangeTooLong, tooLong.Error);
    }
}
