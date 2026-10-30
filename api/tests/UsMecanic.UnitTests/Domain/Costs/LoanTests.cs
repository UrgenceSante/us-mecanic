using UsMecanic.Domain.Costs;

namespace UsMecanic.UnitTests.Domain.Costs;

public class LoanTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    private static LoanEntry Entry(
        string? lender = "Banque Fictive",
        decimal borrowed = 20_000m,
        decimal monthly = 350.50m,
        DateOnly? start = null,
        int months = 60) =>
        new(lender, borrowed, monthly, start ?? new DateOnly(2026, 3, 15), months);

    [Fact]
    public void Cree_un_emprunt_valide_avec_organisme_nettoye()
    {
        var vehicleId = Guid.NewGuid();

        var result = Loan.Create(vehicleId, Entry(lender: "  Banque Fictive "), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(vehicleId, result.Value.VehicleId);
        Assert.Equal("Banque Fictive", result.Value.Lender);
        Assert.Equal(20_000m, result.Value.BorrowedAmount);
        Assert.Equal(350.50m, result.Value.MonthlyPayment);
        Assert.Equal(60, result.Value.DurationMonths);
        Assert.Equal(Now, result.Value.RecordedAt);
    }

    [Fact]
    public void Accepte_des_montants_a_zero()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(borrowed: 0m, monthly: 0m), Now);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Refuse_un_organisme_vide(string? lender)
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(lender: lender), Now);

        Assert.Equal("loan.lender_required", result.Error!.Code);
    }

    [Fact]
    public void Refuse_un_organisme_trop_long()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(lender: new string('a', Loan.MaxLenderLength + 1)), Now);

        Assert.Equal("loan.lender_too_long", result.Error!.Code);
    }

    [Fact]
    public void Refuse_un_montant_emprunte_negatif()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(borrowed: -0.01m), Now);

        Assert.Equal("loan.borrowed_amount_negative", result.Error!.Code);
    }

    [Fact]
    public void Refuse_une_mensualite_negative()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(monthly: -1m), Now);

        Assert.Equal("loan.monthly_payment_negative", result.Error!.Code);
    }

    [Fact]
    public void Refuse_plus_de_deux_decimales()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(monthly: 10.005m), Now);

        Assert.Equal("loan.amount_precision", result.Error!.Code);
    }

    [Fact]
    public void Refuse_un_montant_trop_grand()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(borrowed: Loan.MaxAmount + 0.01m), Now);

        Assert.Equal("loan.amount_too_large", result.Error!.Code);
    }

    [Fact]
    public void Refuse_une_date_de_debut_absente()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(start: default(DateOnly)), Now);

        Assert.Equal("loan.start_date_required", result.Error!.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Refuse_une_duree_nulle_ou_negative(int months)
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(months: months), Now);

        Assert.Equal("loan.duration_invalid", result.Error!.Code);
    }

    [Fact]
    public void Refuse_une_duree_excessive()
    {
        var result = Loan.Create(Guid.NewGuid(), Entry(months: Loan.MaxDurationMonths + 1), Now);

        Assert.Equal("loan.duration_too_long", result.Error!.Code);
    }

    [Fact]
    public void La_modification_change_la_duree_et_conserve_la_saisie()
    {
        var loan = Loan.Create(Guid.NewGuid(), Entry(), Now).Value;

        var result = loan.Update(Entry(months: 24));

        Assert.True(result.IsSuccess);
        Assert.Equal(24, loan.DurationMonths);
        Assert.Equal(Now, loan.RecordedAt);
    }

    [Fact]
    public void Une_modification_invalide_laisse_l_emprunt_inchange()
    {
        var loan = Loan.Create(Guid.NewGuid(), Entry(), Now).Value;

        var result = loan.Update(Entry(months: 0, lender: "Autre"));

        Assert.True(result.IsFailure);
        Assert.Equal(60, loan.DurationMonths);
        Assert.Equal("Banque Fictive", loan.Lender);
    }

    [Fact]
    public void Les_echeances_commencent_le_mois_de_la_date_de_debut_et_couvrent_toute_la_duree()
    {
        var loan = Loan.Create(Guid.NewGuid(), Entry(start: new DateOnly(2026, 11, 30), monthly: 100m, months: 3), Now).Value;

        var installments = loan.Installments();

        Assert.Equal(
            [new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1)],
            installments.Select(i => i.Month));
        Assert.All(installments, i => Assert.Equal(100m, i.Amount));
    }

    [Fact]
    public void Une_duree_raccourcie_retire_les_dernieres_echeances()
    {
        var loan = Loan.Create(Guid.NewGuid(), Entry(start: new DateOnly(2026, 1, 1), months: 12), Now).Value;

        loan.Update(Entry(start: new DateOnly(2026, 1, 1), months: 2));

        Assert.Equal(2, loan.Installments().Count);
        Assert.Equal(new DateOnly(2026, 2, 1), loan.Installments()[^1].Month);
    }
}
