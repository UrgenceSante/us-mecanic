using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.Domain.Deadlines;

public class VehicleDeadlineCompletionTests
{
    private static readonly DateOnly Today = new(2026, 10, 1);
    private static readonly DeadlineType Type = DeadlineType.Create("Contrôle technique", new DeadlineTypeSettings(12, null, null, null)).Value;

    private static VehicleDeadline Periodic(int? months, int? km, DateOnly doneDate, int? doneKm) =>
        VehicleDeadline.Create(Guid.NewGuid(), Type, new VehicleDeadlineSchedule(months, km, doneDate, doneKm, null, null), Today).Value;

    private static VehicleDeadline OneOff(DateOnly? dueDate, int? dueKm) =>
        VehicleDeadline.Create(Guid.NewGuid(), Type, new VehicleDeadlineSchedule(null, null, null, null, dueDate, dueKm), Today).Value;

    [Fact]
    public void Periodique_la_suivante_est_calculee_depuis_la_date_et_le_km_reels()
    {
        var deadline = Periodic(12, 30_000, new DateOnly(2025, 10, 15), 20_000);

        var result = deadline.Complete(new DateOnly(2026, 9, 20), 45_000, Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 9, 20), deadline.LastDoneDate);
        Assert.Equal(45_000, deadline.LastDoneKm);
        Assert.Equal(new DateOnly(2027, 9, 20), deadline.DueDate);
        Assert.Equal(75_000, deadline.DueKm);
        Assert.False(deadline.IsClosed);
    }

    [Fact]
    public void Une_echeance_avec_critere_km_exige_le_km_reel_sans_rien_modifier()
    {
        var deadline = Periodic(12, 30_000, new DateOnly(2025, 10, 15), 20_000);

        var result = deadline.Complete(new DateOnly(2026, 9, 20), null, Today);

        Assert.Equal(DeadlineCompletionErrors.KilometersRequired, result.Error);
        Assert.Equal(new DateOnly(2025, 10, 15), deadline.LastDoneDate);
        Assert.Equal(20_000, deadline.LastDoneKm);
        Assert.Equal(50_000, deadline.DueKm);
    }

    [Fact]
    public void Periodique_par_km_seul_sans_km_reel_est_refusee_sans_rien_modifier()
    {
        var deadline = Periodic(null, 15_000, new DateOnly(2026, 1, 15), 42_000);

        var result = deadline.Complete(new DateOnly(2026, 9, 20), null, Today);

        Assert.Equal(DeadlineCompletionErrors.KilometersRequired, result.Error);
        Assert.Equal(new DateOnly(2026, 1, 15), deadline.LastDoneDate);
        Assert.Equal(57_000, deadline.DueKm);
    }

    [Fact]
    public void Ponctuelle_est_close_et_conserve_son_echeance()
    {
        var deadline = OneOff(new DateOnly(2026, 12, 1), 60_000);

        var result = deadline.Complete(new DateOnly(2026, 9, 30), 55_000, Today);

        Assert.True(result.IsSuccess);
        Assert.True(deadline.IsClosed);
        Assert.Equal(new DateOnly(2026, 9, 30), deadline.LastDoneDate);
        Assert.Equal(55_000, deadline.LastDoneKm);
        Assert.Equal(new DateOnly(2026, 12, 1), deadline.DueDate);
        Assert.Null(deadline.StatusAt(55_000, Today, new DueSoonLimits(30, 1000)));
    }

    [Fact]
    public void Une_echeance_close_ne_peut_plus_etre_realisee()
    {
        var deadline = OneOff(new DateOnly(2026, 12, 1), null);
        deadline.Complete(Today, null, Today);

        var again = deadline.Complete(Today, null, Today);

        Assert.Equal(VehicleDeadlineErrors.Closed, again.Error);
    }

    [Fact]
    public void Une_date_future_est_refusee_sans_rien_modifier()
    {
        var deadline = OneOff(new DateOnly(2026, 12, 1), null);

        var result = deadline.Complete(Today.AddDays(1), null, Today);

        Assert.Equal(DeadlineCompletionErrors.DateInFuture, result.Error);
        Assert.False(deadline.IsClosed);
        Assert.Null(deadline.LastDoneDate);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(VehicleDeadline.MaxKilometers + 1)]
    public void Un_km_hors_bornes_est_refuse(int kilometers)
    {
        var deadline = OneOff(new DateOnly(2026, 12, 1), null);

        var result = deadline.Complete(Today, kilometers, Today);

        Assert.Equal(VehicleDeadlineErrors.KilometersInvalid, result.Error);
        Assert.False(deadline.IsClosed);
    }

    [Fact]
    public void La_realisation_nettoie_le_commentaire_et_refuse_un_commentaire_trop_long()
    {
        var now = new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);
        var id = Guid.NewGuid();

        var ok = DeadlineCompletion.Create(id, Today, 1_000, "  Fait au garage ", now);
        var blank = DeadlineCompletion.Create(id, Today, null, "   ", now);
        var tooLong = DeadlineCompletion.Create(id, Today, null, new string('x', DeadlineCompletion.MaxCommentLength + 1), now);

        Assert.Equal("Fait au garage", ok.Value.Comment);
        Assert.Equal(now, ok.Value.RecordedAt);
        Assert.Equal(id, ok.Value.VehicleDeadlineId);
        Assert.Null(blank.Value.Comment);
        Assert.Equal(DeadlineCompletionErrors.CommentTooLong, tooLong.Error);
    }
}
