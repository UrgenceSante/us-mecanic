using System.Reflection;
using UsMecanic.Application.Costs.Expenses;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Costs;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Costs;

public class ExpenseUseCasesTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryExpenseCategoryRepository _categories = new();
    private readonly InMemoryExpenseRepository _expenses = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;
    private readonly ExpenseCategory _cleaning;

    public ExpenseUseCasesTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
        _cleaning = _categories.Seed("Nettoyage");
    }

    private ExpenseDtoFactory Factory => new(_categories);

    private AddExpense Add => new(new MileageVehicleGuard(_vehicles), _categories, _expenses, Factory, _clock, _clock.Calendar);

    private UpdateExpense Update => new(new MileageVehicleGuard(_vehicles), _categories, _expenses, Factory, _clock.Calendar);

    private DeleteExpense Delete => new(new MileageVehicleGuard(_vehicles), _expenses);

    private ListExpenses List => new(new MileageVehicleGuard(_vehicles), _expenses, Factory);

    private ExpenseInput Input(DateOnly? date = null, decimal? amount = 10m, Guid? categoryId = null, string? label = "Lavage") =>
        new(date ?? Today, categoryId ?? _cleaning.Id, amount, label);

    [Fact]
    public async Task Ajoute_une_depense_avec_le_nom_de_sa_categorie()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, Input(amount: 12.34m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", result.Value.CategoryName);
        Assert.Equal(12.34m, result.Value.Amount);
        Assert.Equal(ExpenseSource.Manual, result.Value.Source);
        Assert.Equal(ExpenseFamily.OneOff, result.Value.Family);
        Assert.Equal(_clock.Now, result.Value.RecordedAt);
        Assert.Single(_expenses.Saved);
    }

    [Fact]
    public async Task Refuse_une_categorie_inconnue_inactive_ou_absente_sans_rien_enregistrer()
    {
        var inactive = _categories.Seed("Ancienne", active: false);

        var unknown = await Add.ExecuteAsync(_vehicle.Id, Input(categoryId: Guid.NewGuid()), CancellationToken.None);
        var disabled = await Add.ExecuteAsync(_vehicle.Id, Input(categoryId: inactive.Id), CancellationToken.None);
        var missing = await Add.ExecuteAsync(_vehicle.Id, new ExpenseInput(Today, null, 1m), CancellationToken.None);

        Assert.Equal(ExpenseCategoryErrors.Unknown, unknown.Error);
        Assert.Equal(ExpenseCategoryErrors.Inactive, disabled.Error);
        Assert.Equal(ExpenseErrors.CategoryRequired, missing.Error);
        Assert.Empty(_expenses.Saved);
        Assert.Equal(0, _expenses.SaveCount);
    }

    [Fact]
    public async Task Refuse_la_saisie_de_la_famille_utilisation_courante()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, Input() with { Family = ExpenseFamily.Running }, CancellationToken.None);

        Assert.Equal(ExpenseErrors.FamilyNotManual, result.Error);
        Assert.Empty(_expenses.Saved);
    }

    [Fact]
    public async Task Refuse_une_date_future()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, Input(date: Today.AddDays(1)), CancellationToken.None);

        Assert.Equal(ExpenseErrors.DateInFuture, result.Error);
    }

    [Fact]
    public async Task Vehicule_inconnu_ou_sorti_du_parc()
    {
        var unknown = await Add.ExecuteAsync(Guid.NewGuid(), Input(), CancellationToken.None);
        var unknownList = await List.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);
        var retiredAdd = await Add.ExecuteAsync(_vehicle.Id, Input(), CancellationToken.None);
        var retiredList = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, unknown.Error);
        Assert.Equal(VehicleErrors.NotFound, unknownList.Error);
        Assert.Equal(VehicleErrors.Retired, retiredAdd.Error);
        Assert.True(retiredList.IsSuccess);
    }

    [Fact]
    public async Task Liste_les_depenses_du_vehicule_par_date_decroissante()
    {
        var other = _vehicles.Seed("EF-456-GH");
        await Add.ExecuteAsync(_vehicle.Id, Input(date: Today.AddDays(-5), label: "ancienne"), CancellationToken.None);
        await Add.ExecuteAsync(_vehicle.Id, Input(date: Today.AddDays(-1), label: "récente"), CancellationToken.None);
        await Add.ExecuteAsync(other.Id, Input(label: "autre véhicule"), CancellationToken.None);

        var result = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        List<string?> labels = [.. result.Value.Select(e => e.Label)];
        Assert.Equal(2, labels.Count);
        Assert.Equal("récente", labels[0]);
        Assert.Equal("ancienne", labels[1]);
    }

    [Fact]
    public async Task Modifie_une_depense()
    {
        var other = _categories.Seed("Divers");
        var created = await Add.ExecuteAsync(_vehicle.Id, Input(), CancellationToken.None);

        var result = await Update.ExecuteAsync(_vehicle.Id, created.Value.Id, Input(amount: 0m, categoryId: other.Id, label: null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.Amount);
        Assert.Equal("Divers", result.Value.CategoryName);
        Assert.Null(result.Value.Label);
        Assert.Equal(created.Value.RecordedAt, result.Value.RecordedAt);
    }

    [Fact]
    public async Task Garde_une_categorie_inchangee_meme_desactivee_mais_refuse_une_nouvelle_categorie_inactive()
    {
        var created = await Add.ExecuteAsync(_vehicle.Id, Input(), CancellationToken.None);
        _cleaning.Deactivate();
        var inactive = _categories.Seed("Ancienne", active: false);

        var same = await Update.ExecuteAsync(_vehicle.Id, created.Value.Id, Input(amount: 20m), CancellationToken.None);
        var changed = await Update.ExecuteAsync(_vehicle.Id, created.Value.Id, Input(categoryId: inactive.Id), CancellationToken.None);

        Assert.True(same.IsSuccess);
        Assert.Equal(ExpenseCategoryErrors.Inactive, changed.Error);
    }

    [Fact]
    public async Task Modification_et_suppression_refusees_pour_une_depense_inconnue_ou_d_un_autre_vehicule()
    {
        var other = _vehicles.Seed("EF-456-GH");
        var created = await Add.ExecuteAsync(other.Id, Input(), CancellationToken.None);

        var update = await Update.ExecuteAsync(_vehicle.Id, created.Value.Id, Input(), CancellationToken.None);
        var delete = await Delete.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ExpenseErrors.NotFound, update.Error);
        Assert.Equal(ExpenseErrors.NotFound, delete.Error);
    }

    [Fact]
    public async Task Supprime_une_depense()
    {
        var created = await Add.ExecuteAsync(_vehicle.Id, Input(), CancellationToken.None);

        var result = await Delete.ExecuteAsync(_vehicle.Id, created.Value.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_expenses.Saved);
    }

    [Fact]
    public async Task Refuse_de_modifier_ou_supprimer_une_depense_issue_d_un_connecteur()
    {
        var created = await Add.ExecuteAsync(_vehicle.Id, Input(), CancellationToken.None);
        var expense = _expenses.Saved.Single();
        typeof(Expense).GetField("<Source>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(expense, ExpenseSource.Ulys);

        var update = await Update.ExecuteAsync(_vehicle.Id, created.Value.Id, Input(amount: 99m), CancellationToken.None);
        var delete = await Delete.ExecuteAsync(_vehicle.Id, created.Value.Id, CancellationToken.None);

        Assert.Equal(ExpenseErrors.NotManual, update.Error);
        Assert.Equal(ExpenseErrors.NotManual, delete.Error);
        Assert.Equal(10m, expense.Amount);
        Assert.Single(_expenses.Saved);
    }
}
