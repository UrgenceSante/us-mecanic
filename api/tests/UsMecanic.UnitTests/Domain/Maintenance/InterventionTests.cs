using System.Globalization;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Maintenance;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Maintenance;

public class InterventionTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;
    private static readonly DateTimeOffset Now = FixedClock.Default;

    private static Result<Intervention> Create(
        string? work = "Vidange", DateOnly? date = null, IReadOnlyCollection<Guid>? deadlineIds = null, int? kilometers = null, Guid? garageId = null,
        decimal? amount = null) =>
        Intervention.Create(Guid.NewGuid(), date ?? Today, kilometers, garageId, work, deadlineIds ?? [], Today, Now, amount);

    [Fact]
    public void Create_nettoie_les_travaux_et_conserve_les_echeances()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var garageId = Guid.NewGuid();

        var result = Create("  Vidange et filtres ", Today.AddDays(-2), [first, second], 42_000, garageId);

        Assert.True(result.IsSuccess);
        Assert.Equal("Vidange et filtres", result.Value.Work);
        Assert.Equal(Today.AddDays(-2), result.Value.Date);
        Assert.Equal(42_000, result.Value.Kilometers);
        Assert.Equal(garageId, result.Value.GarageId);
        Assert.Equal(Now, result.Value.RecordedAt);
        Assert.Equal(new[] { first, second }, result.Value.Deadlines.Select(d => d.VehicleDeadlineId).ToArray());
        Assert.All(result.Value.Deadlines, d => Assert.Equal(result.Value.Id, d.InterventionId));
    }

    [Fact]
    public void Create_accepte_une_intervention_sans_echeance_ni_km_ni_garage()
    {
        var result = Create();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Deadlines);
        Assert.Null(result.Value.Kilometers);
        Assert.Null(result.Value.GarageId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_des_travaux_vides(string? work)
    {
        Assert.Equal(InterventionErrors.WorkRequired, Create(work).Error);
    }

    [Fact]
    public void Create_borne_la_longueur_des_travaux()
    {
        Assert.True(Create(new string('x', Intervention.MaxWorkLength)).IsSuccess);
        Assert.Equal(InterventionErrors.WorkTooLong, Create(new string('x', Intervention.MaxWorkLength + 1)).Error);
    }

    [Fact]
    public void Create_refuse_une_date_future_mais_accepte_aujourd_hui()
    {
        Assert.True(Create(date: Today).IsSuccess);
        Assert.Equal(InterventionErrors.DateInFuture, Create(date: Today.AddDays(1)).Error);
    }

    [Fact]
    public void Create_refuse_une_echeance_en_double()
    {
        var id = Guid.NewGuid();

        Assert.Equal(InterventionErrors.DeadlineDuplicated, Create(deadlineIds: [id, Guid.NewGuid(), id]).Error);
    }

    [Fact]
    public void Create_sans_montant_laisse_le_montant_absent()
    {
        Assert.Null(Create().Value.Amount);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("120.5")]
    [InlineData("120.50")]
    [InlineData("0.01")]
    [InlineData("120.500")]
    public void Create_accepte_un_montant_positif_ou_nul_a_deux_decimales_au_plus(string amount)
    {
        var expected = decimal.Parse(amount, CultureInfo.InvariantCulture);

        var result = Create(amount: expected);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Amount);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("-120")]
    public void Create_refuse_un_montant_negatif(string amount)
    {
        var result = Create(amount: decimal.Parse(amount, CultureInfo.InvariantCulture));

        Assert.Equal(InterventionErrors.AmountNegative, result.Error);
    }

    [Theory]
    [InlineData("0.001")]
    [InlineData("120.505")]
    public void Create_refuse_un_montant_a_plus_de_deux_decimales(string amount)
    {
        var result = Create(amount: decimal.Parse(amount, CultureInfo.InvariantCulture));

        Assert.Equal(InterventionErrors.AmountTooPrecise, result.Error);
    }
}
