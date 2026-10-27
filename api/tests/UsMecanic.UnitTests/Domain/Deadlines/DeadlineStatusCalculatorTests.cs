using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.Domain.Deadlines;

public class DeadlineStatusCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 10, 1);
    private static readonly DueSoonLimits Limits = new(30, 1000);

    private static DeadlineStatus Compute(int? daysFromToday, int? dueKm, int? currentKm) =>
        DeadlineStatusCalculator.Compute(daysFromToday is { } d ? Today.AddDays(d) : null, dueKm, currentKm, Today, Limits);

    [Theory]
    [InlineData(-1, DeadlineStatus.Overdue)]
    [InlineData(0, DeadlineStatus.DueSoon)] // le jour même, pas encore dépassée
    [InlineData(1, DeadlineStatus.DueSoon)]
    [InlineData(30, DeadlineStatus.DueSoon)] // égal au seuil
    [InlineData(31, DeadlineStatus.UpToDate)]
    public void Statut_selon_la_date(int daysFromToday, DeadlineStatus expected) =>
        Assert.Equal(expected, Compute(daysFromToday, null, null));

    [Theory]
    [InlineData(10_001, DeadlineStatus.Overdue)] // km actuel au-delà de l'échéance
    [InlineData(10_000, DeadlineStatus.Overdue)] // km égal à l'échéance
    [InlineData(9_999, DeadlineStatus.DueSoon)]
    [InlineData(9_000, DeadlineStatus.DueSoon)] // 1 000 km restants = seuil
    [InlineData(8_999, DeadlineStatus.UpToDate)]
    public void Statut_selon_le_km(int currentKm, DeadlineStatus expected) =>
        Assert.Equal(expected, Compute(null, 10_000, currentKm));

    [Fact]
    public void Double_critere_le_premier_atteint_l_emporte()
    {
        // Date lointaine mais km dépassé : dépassée.
        Assert.Equal(DeadlineStatus.Overdue, Compute(200, 10_000, 10_000));

        // Date dépassée mais km lointain : dépassée.
        Assert.Equal(DeadlineStatus.Overdue, Compute(-1, 50_000, 10_000));

        // Date proche et km lointain : proche.
        Assert.Equal(DeadlineStatus.DueSoon, Compute(10, 50_000, 10_000));

        // Date lointaine et km proche : proche.
        Assert.Equal(DeadlineStatus.DueSoon, Compute(200, 10_500, 10_000));

        // Les deux lointains : à jour.
        Assert.Equal(DeadlineStatus.UpToDate, Compute(200, 50_000, 10_000));
    }

    [Fact]
    public void Sans_km_actuel_le_critere_km_est_ignore()
    {
        Assert.Equal(DeadlineStatus.UpToDate, Compute(null, 100, null));
        Assert.Equal(DeadlineStatus.UpToDate, Compute(200, 100, null));
        Assert.Equal(DeadlineStatus.Overdue, Compute(-1, 100, null));
        Assert.Equal(DeadlineStatus.DueSoon, Compute(5, 100, null));
    }

    [Fact]
    public void Sans_critere_l_echeance_est_a_jour() =>
        Assert.Equal(DeadlineStatus.UpToDate, Compute(null, null, 10_000));

    [Fact]
    public void Les_seuils_du_type_l_emportent_sur_les_globaux()
    {
        var global = DeadlineThresholds.CreateDefault();
        var own = DeadlineType.Create("Assurance", new DeadlineTypeSettings(12, null, 60, null)).Value;
        var plain = DeadlineType.Create("Oxygène", null).Value;

        Assert.Equal(new DueSoonLimits(60, DeadlineThresholds.DefaultKm), DueSoonLimits.For(own, global));
        Assert.Equal(new DueSoonLimits(DeadlineThresholds.DefaultDays, DeadlineThresholds.DefaultKm), DueSoonLimits.For(plain, global));
    }

    [Fact]
    public void Un_seuil_propre_au_type_change_le_statut()
    {
        var wide = new DueSoonLimits(90, 1000);

        Assert.Equal(DeadlineStatus.UpToDate, DeadlineStatusCalculator.Compute(Today.AddDays(60), null, null, Today, Limits));
        Assert.Equal(DeadlineStatus.DueSoon, DeadlineStatusCalculator.Compute(Today.AddDays(60), null, null, Today, wide));
    }
}
