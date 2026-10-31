using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.UnitTests.Domain.Costs;

public class ExpenseTests
{
    private static readonly DateOnly Today = new(2026, 10, 1);
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly Guid VehicleId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    private static Expense Create() =>
        Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, 42.5m, "Lavage", Now).Value;

    [Fact]
    public void Cree_une_depense_manuelle_ponctuelle()
    {
        var result = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, 42.5m, "  Lavage  ", Now);

        Assert.True(result.IsSuccess);
        var expense = result.Value;
        Assert.Equal(VehicleId, expense.VehicleId);
        Assert.Equal(ExpenseFamily.OneOff, expense.Family);
        Assert.Equal(ExpenseSource.Manual, expense.Source);
        Assert.Equal(CategoryId, expense.CategoryId);
        Assert.Equal(42.5m, expense.Amount);
        Assert.Equal("Lavage", expense.Label);
        Assert.Equal(Now, expense.RecordedAt);
    }

    [Fact]
    public void Accepte_un_montant_nul_et_le_plafond()
    {
        Assert.True(Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, 0m, null, Now).IsSuccess);
        Assert.True(Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, Expense.MaxAmount, null, Now).IsSuccess);
    }

    [Fact]
    public void Refuse_un_montant_negatif_trop_precis_ou_trop_grand_ou_absent()
    {
        AssertAmountError(amount: -0.01m, ExpenseErrors.AmountNegative);
        AssertAmountError(amount: 1.005m, ExpenseErrors.AmountTooPrecise);
        AssertAmountError(amount: Expense.MaxAmount + 0.01m, ExpenseErrors.AmountTooLarge);
        AssertAmountError(amount: null, ExpenseErrors.AmountRequired);
    }

    private static void AssertAmountError(decimal? amount, DomainError expected)
    {
        var result = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, amount, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(expected, result.Error);
    }

    [Fact]
    public void Refuse_une_date_absente_et_accepte_une_date_future()
    {
        var missing = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, null, CategoryId, 1m, null, Now);
        var future = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today.AddDays(1), CategoryId, 1m, null, Now);
        var todayOk = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, 1m, null, Now);

        Assert.Equal(ExpenseErrors.DateRequired, missing.Error);
        Assert.True(future.IsSuccess);
        Assert.True(todayOk.IsSuccess);
    }

    [Fact]
    public void Refuse_une_categorie_absente()
    {
        var result = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, null, 1m, null, Now);

        Assert.Equal(ExpenseErrors.CategoryRequired, result.Error);
    }

    [Fact]
    public void Refuse_la_saisie_manuelle_de_la_famille_utilisation_courante()
    {
        var result = Expense.CreateManual(VehicleId, ExpenseFamily.Running, Today, CategoryId, 1m, null, Now);

        Assert.Equal(ExpenseErrors.FamilyNotManual, result.Error);
    }

    [Fact]
    public void Limite_le_libelle_et_ignore_un_libelle_blanc()
    {
        var atLimit = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, 1m, new string('x', Expense.MaxLabelLength), Now);
        var tooLong = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, 1m, new string('x', Expense.MaxLabelLength + 1), Now);
        var blank = Expense.CreateManual(VehicleId, ExpenseFamily.OneOff, Today, CategoryId, 1m, "   ", Now);

        Assert.True(atLimit.IsSuccess);
        Assert.Equal(ExpenseErrors.LabelTooLong, tooLong.Error);
        Assert.Null(blank.Value.Label);
    }

    [Fact]
    public void Modifie_une_depense_manuelle()
    {
        var expense = Create();
        var otherCategory = Guid.NewGuid();

        var result = expense.Update(Today.AddDays(-2), otherCategory, 0m, " Autre ");

        Assert.True(result.IsSuccess);
        Assert.Equal(Today.AddDays(-2), expense.Date);
        Assert.Equal(otherCategory, expense.CategoryId);
        Assert.Equal(0m, expense.Amount);
        Assert.Equal("Autre", expense.Label);
    }

    [Fact]
    public void Ne_modifie_rien_si_la_modification_est_refusee()
    {
        var expense = Create();

        var result = expense.Update(Today, CategoryId, -1m, "Changé");

        Assert.Equal(ExpenseErrors.AmountNegative, result.Error);
        Assert.Equal(42.5m, expense.Amount);
        Assert.Equal("Lavage", expense.Label);
    }
}
