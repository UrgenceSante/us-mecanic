using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.Domain.Deadlines;

public class VehicleDeadlineTests
{
    private static readonly DateOnly Today = new(2026, 10, 1);
    private static readonly Guid VehicleId = Guid.NewGuid();
    private static readonly DeadlineType Type = DeadlineType.Create("Contrôle technique", new DeadlineTypeSettings(12, null, null, null)).Value;

    private static VehicleDeadlineSchedule Periodic(int? months, int? km, DateOnly? doneDate, int? doneKm) =>
        new(months, km, doneDate, doneKm, null, null);

    private static VehicleDeadlineSchedule OneOff(DateOnly? dueDate, int? dueKm) =>
        new(null, null, null, null, dueDate, dueKm);

    [Fact]
    public void Periodique_la_prochaine_echeance_est_la_derniere_realisation_plus_la_periodicite()
    {
        var result = VehicleDeadline.Create(VehicleId, Type, Periodic(12, 30_000, new DateOnly(2026, 1, 15), 20_000), Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2027, 1, 15), result.Value.DueDate);
        Assert.Equal(50_000, result.Value.DueKm);
        Assert.Equal(VehicleId, result.Value.VehicleId);
        Assert.Equal(Type.Id, result.Value.DeadlineTypeId);
        Assert.False(result.Value.IsClosed);
    }

    [Fact]
    public void Periodique_les_mois_utilisent_AddMonths_en_fin_de_mois()
    {
        var result = VehicleDeadline.Create(VehicleId, Type, Periodic(1, null, new DateOnly(2026, 1, 31), null), Today);

        Assert.Equal(new DateOnly(2026, 2, 28), result.Value.DueDate);
        Assert.Null(result.Value.DueKm);
    }

    [Fact]
    public void Periodique_par_km_seul_ne_calcule_que_le_km()
    {
        var result = VehicleDeadline.Create(VehicleId, Type, Periodic(null, 15_000, new DateOnly(2026, 1, 15), 42_000), Today);

        Assert.Null(result.Value.DueDate);
        Assert.Equal(57_000, result.Value.DueKm);
    }

    [Fact]
    public void Periodique_ignore_les_dates_et_km_d_echeance_saisis()
    {
        var schedule = Periodic(12, null, new DateOnly(2026, 1, 15), null) with { DueDate = new DateOnly(2030, 1, 1), DueKm = 1 };

        var result = VehicleDeadline.Create(VehicleId, Type, schedule, Today);

        Assert.Equal(new DateOnly(2027, 1, 15), result.Value.DueDate);
        Assert.Null(result.Value.DueKm);
    }

    [Fact]
    public void Periodique_sans_derniere_realisation_correspondante_est_refusee()
    {
        Assert.Equal(VehicleDeadlineErrors.CriterionRequired, VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, null, null), Today).Error);
        Assert.Equal(VehicleDeadlineErrors.CriterionRequired, VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, null, 5_000), Today).Error);
        Assert.Equal(VehicleDeadlineErrors.CriterionRequired, VehicleDeadline.Create(VehicleId, Type, Periodic(null, 10_000, new DateOnly(2026, 1, 1), null), Today).Error);
    }

    [Fact]
    public void Ponctuelle_date_et_ou_km_sont_saisis_directement()
    {
        var both = VehicleDeadline.Create(VehicleId, Type, OneOff(new DateOnly(2027, 3, 1), 80_000), Today).Value;
        var dateOnly = VehicleDeadline.Create(VehicleId, Type, OneOff(new DateOnly(2027, 3, 1), null), Today).Value;
        var kmOnly = VehicleDeadline.Create(VehicleId, Type, OneOff(null, 80_000), Today).Value;

        Assert.Equal(new DateOnly(2027, 3, 1), both.DueDate);
        Assert.Equal(80_000, both.DueKm);
        Assert.Equal(new DateOnly(2027, 3, 1), dateOnly.DueDate);
        Assert.Null(dateOnly.DueKm);
        Assert.Equal(80_000, kmOnly.DueKm);
        Assert.Null(kmOnly.DueDate);
    }

    [Fact]
    public void Au_moins_un_critere_est_requis()
    {
        var result = VehicleDeadline.Create(VehicleId, Type, OneOff(null, null), Today);

        Assert.Equal(VehicleDeadlineErrors.CriterionRequired, result.Error);
    }

    [Fact]
    public void Un_type_inactif_est_refuse()
    {
        var inactive = DeadlineType.Create("Ancien", null).Value;
        inactive.Deactivate();

        var result = VehicleDeadline.Create(VehicleId, inactive, OneOff(Today, null), Today);

        Assert.Equal(VehicleDeadlineErrors.TypeInactive, result.Error);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(-3, null)]
    [InlineData(VehicleDeadline.MaxIntervalMonths + 1, null)]
    [InlineData(null, 0)]
    [InlineData(null, -1)]
    public void Les_periodicites_non_strictement_positives_sont_refusees(int? months, int? km)
    {
        var result = VehicleDeadline.Create(VehicleId, Type, Periodic(months, km, new DateOnly(2026, 1, 1), 1_000), Today);

        Assert.True(result.IsFailure);
        Assert.True(result.Error == VehicleDeadlineErrors.IntervalMonthsInvalid || result.Error == VehicleDeadlineErrors.IntervalKmInvalid);
    }

    [Fact]
    public void Les_kilometrages_negatifs_ou_demesures_sont_refuses()
    {
        Assert.Equal(VehicleDeadlineErrors.KilometersInvalid, VehicleDeadline.Create(VehicleId, Type, OneOff(null, -1), Today).Error);
        Assert.Equal(VehicleDeadlineErrors.KilometersInvalid, VehicleDeadline.Create(VehicleId, Type, OneOff(null, VehicleDeadline.MaxKilometers + 1), Today).Error);
        Assert.Equal(VehicleDeadlineErrors.KilometersInvalid, VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, Today, -5), Today).Error);
    }

    [Fact]
    public void La_derniere_realisation_dans_le_futur_est_refusee_mais_aujourd_hui_est_accepte()
    {
        Assert.Equal(VehicleDeadlineErrors.LastDoneInFuture, VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, Today.AddDays(1), null), Today).Error);
        Assert.True(VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, Today, null), Today).IsSuccess);
    }

    [Fact]
    public void Changer_la_periodicite_recalcule_la_prochaine_echeance()
    {
        var deadline = VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, new DateOnly(2026, 1, 15), 20_000), Today).Value;

        var result = deadline.ChangeInterval(6, 10_000, Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, deadline.IntervalMonths);
        Assert.Equal(10_000, deadline.IntervalKm);
        Assert.Equal(new DateOnly(2026, 7, 15), deadline.DueDate);
        Assert.Equal(30_000, deadline.DueKm);
        Assert.Equal(new DateOnly(2026, 1, 15), deadline.LastDoneDate);
    }

    [Fact]
    public void Vider_la_periodicite_rend_l_echeance_ponctuelle_en_gardant_les_echeances()
    {
        var deadline = VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, new DateOnly(2026, 1, 15), null), Today).Value;

        var result = deadline.ChangeInterval(null, null, Today);

        Assert.True(result.IsSuccess);
        Assert.Null(deadline.IntervalMonths);
        Assert.Equal(new DateOnly(2027, 1, 15), deadline.DueDate);
    }

    [Fact]
    public void Un_changement_de_periodicite_invalide_ne_modifie_rien()
    {
        var deadline = VehicleDeadline.Create(VehicleId, Type, Periodic(12, null, new DateOnly(2026, 1, 15), null), Today).Value;

        var invalid = deadline.ChangeInterval(0, null, Today);
        var withoutCriterion = deadline.ChangeInterval(null, 10_000, Today);

        Assert.Equal(VehicleDeadlineErrors.IntervalMonthsInvalid, invalid.Error);
        Assert.Equal(VehicleDeadlineErrors.CriterionRequired, withoutCriterion.Error);
        Assert.Equal(12, deadline.IntervalMonths);
        Assert.Null(deadline.IntervalKm);
        Assert.Equal(new DateOnly(2027, 1, 15), deadline.DueDate);
    }

    [Fact]
    public void Le_statut_est_calcule_sur_la_date_et_le_km_de_l_echeance()
    {
        var deadline = VehicleDeadline.Create(VehicleId, Type, OneOff(Today, 10_000), Today).Value;
        var limits = new DueSoonLimits(30, 1000);

        Assert.Equal(DeadlineStatus.DueSoon, deadline.StatusAt(5_000, Today, limits));
        Assert.Equal(DeadlineStatus.Overdue, deadline.StatusAt(10_000, Today, limits));
        Assert.Equal(DeadlineStatus.Overdue, deadline.StatusAt(null, Today.AddDays(1), limits));
    }
}
