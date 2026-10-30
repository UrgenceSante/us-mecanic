using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Mileage;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Deadlines;

public class CompleteVehicleDeadlineTests
{
    private static readonly DateOnly Today = new(2026, 10, 1); // FixedClock.Default

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryDeadlineTypeRepository _types = new();
    private readonly InMemoryDeadlineThresholdsRepository _thresholds = new();
    private readonly InMemoryMileageReadingRepository _readings = new();
    private readonly InMemoryVehicleDeadlineRepository _deadlines = new();
    private readonly InMemoryGarageRepository _garages = new();
    private readonly InMemoryInterventionRepository _interventions = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;
    private readonly DeadlineType _type;

    public CompleteVehicleDeadlineTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
        _type = _types.Seed("Contrôle technique");
        _interventions.SharedSaves.Add(() => _readings.SaveChangesAsync(CancellationToken.None));
        _interventions.SharedSaves.Add(() => _deadlines.SaveChangesAsync(CancellationToken.None));
    }

    private VehicleDeadlineDtoFactory Factory => new(_types, _thresholds, _readings, _clock.Calendar);

    private CompleteVehicleDeadline Complete
    {
        get
        {
            var guard = new MileageVehicleGuard(_vehicles);
            var add = new AddIntervention(
                guard, _garages, _deadlines, _readings, _interventions, new InterventionDtoFactory(_garages, _deadlines, _types), _clock, _clock.Calendar);
            return new CompleteVehicleDeadline(guard, _deadlines, _types, add, Factory);
        }
    }

    private async Task<VehicleDeadlineDto> AddAsync(VehicleDeadlineInput input)
    {
        var add = new AddVehicleDeadline(new MileageVehicleGuard(_vehicles), _types, _deadlines, Factory, _clock.Calendar);
        return (await add.ExecuteAsync(_vehicle.Id, input, CancellationToken.None)).Value;
    }

    private Task<VehicleDeadlineDto> AddPeriodicAsync() =>
        AddAsync(new VehicleDeadlineInput(_type.Id, 12, 30_000, Today.AddMonths(-11), 20_000));

    private Task<VehicleDeadlineDto> AddOneOffAsync() =>
        AddAsync(new VehicleDeadlineInput(_type.Id, DueDate: Today.AddDays(10)));

    [Fact]
    public async Task Periodique_recalcule_la_suivante_depuis_la_date_et_le_km_reels_et_cree_le_releve()
    {
        var deadline = await AddPeriodicAsync();
        var date = Today.AddDays(-5);

        var result = await Complete.ExecuteAsync(
            _vehicle.Id, deadline.Id, new DeadlineCompletionInput(date, 21_000, " Révision complète "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(date, result.Value.LastDoneDate);
        Assert.Equal(21_000, result.Value.LastDoneKm);
        Assert.Equal(date.AddMonths(12), result.Value.DueDate);
        Assert.Equal(51_000, result.Value.DueKm);
        Assert.False(result.Value.IsClosed);
        Assert.Equal(DeadlineStatus.UpToDate, result.Value.Status);
        var reading = Assert.Single(_readings.Saved);
        Assert.Equal((date, 21_000), (reading.Date, reading.Kilometers));
        var intervention = Assert.Single(_interventions.Saved);
        Assert.Equal(deadline.Id, Assert.Single(intervention.Deadlines).VehicleDeadlineId);
        Assert.Equal("Révision complète", intervention.Work);
        Assert.Equal((date, 21_000), (intervention.Date, intervention.Kilometers));
        Assert.Equal(_clock.Now, intervention.RecordedAt);
        Assert.Null(intervention.GarageId);
    }

    [Fact]
    public async Task Ponctuelle_est_close_sans_nouvelle_echeance_et_sans_statut()
    {
        var deadline = await AddOneOffAsync();

        var result = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsClosed);
        Assert.Null(result.Value.Status);
        Assert.Equal(Today, result.Value.LastDoneDate);
        Assert.Single(_deadlines.Saved);
        Assert.Empty(_readings.Saved);
        var intervention = Assert.Single(_interventions.Saved);
        Assert.Equal("Contrôle technique", intervention.Work);
    }

    [Fact]
    public async Task Le_garage_est_transmis_et_un_garage_inconnu_ou_inactif_est_refuse()
    {
        var deadline = await AddOneOffAsync();
        var garage = _garages.Seed("Garage Martin");

        var unknown = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today, GarageId: Guid.NewGuid()), CancellationToken.None);
        var ok = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today, GarageId: garage.Id), CancellationToken.None);

        Assert.Equal(InterventionErrors.GarageUnknown, unknown.Error);
        Assert.True(ok.IsSuccess);
        Assert.Equal(garage.Id, Assert.Single(_interventions.Saved).GarageId);
    }

    [Fact]
    public async Task Un_km_incoherent_sans_motif_est_refuse_sans_rien_enregistrer()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-30), 30_000);
        var deadline = await AddPeriodicAsync();
        var saves = _deadlines.SaveCount;

        var result = await Complete.ExecuteAsync(
            _vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today.AddDays(-5), 25_000), CancellationToken.None);

        Assert.Equal(MileageReadingErrors.ReasonRequired, result.Error);
        Assert.Equal(saves, _deadlines.SaveCount);
        Assert.Single(_readings.Saved);
        Assert.Empty(_interventions.Saved);
        Assert.Equal(Today.AddMonths(-11), _deadlines.Saved[0].LastDoneDate);
    }

    [Fact]
    public async Task Le_motif_et_la_confirmation_sont_transmis_au_releve()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-30), 30_000);
        var deadline = await AddPeriodicAsync();

        var withoutConfirmation = await Complete.ExecuteAsync(
            _vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today.AddDays(-29), 50_000), CancellationToken.None);
        var withReason = await Complete.ExecuteAsync(
            _vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today.AddDays(-5), 25_000, Reason: "Compteur remplacé"), CancellationToken.None);

        Assert.Equal(MileageReadingErrors.HighMileageConfirmationRequired, withoutConfirmation.Error);
        Assert.True(withReason.IsSuccess);
        Assert.Equal("Compteur remplacé", _readings.Saved.Find(r => r.Kilometers == 25_000)?.Reason);
    }

    [Fact]
    public async Task Une_date_future_est_refusee()
    {
        var deadline = await AddOneOffAsync();

        var withoutKm = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today.AddDays(1)), CancellationToken.None);
        var withKm = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today.AddDays(1), 1_000), CancellationToken.None);

        Assert.Equal(DeadlineCompletionErrors.DateInFuture, withoutKm.Error);
        Assert.Equal(DeadlineCompletionErrors.DateInFuture, withKm.Error);
        Assert.Empty(_interventions.Saved);
        Assert.False(_deadlines.Saved[0].IsClosed);
    }

    [Fact]
    public async Task Une_echeance_close_ou_inconnue_est_refusee()
    {
        var deadline = await AddOneOffAsync();
        await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today), CancellationToken.None);

        var closed = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today, 500), CancellationToken.None);
        var unknown = await Complete.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), new DeadlineCompletionInput(Today), CancellationToken.None);

        Assert.Equal(VehicleDeadlineErrors.Closed, closed.Error);
        Assert.Equal(VehicleDeadlineErrors.NotFound, unknown.Error);
        Assert.Single(_interventions.Saved);
        Assert.Empty(_readings.Saved);
    }

    [Fact]
    public async Task Un_commentaire_trop_long_est_refuse()
    {
        var deadline = await AddOneOffAsync();
        var input = new DeadlineCompletionInput(Today, Comment: new string('x', DeadlineCompletionErrors.MaxCommentLength + 1));

        var result = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, input, CancellationToken.None);

        Assert.Equal(DeadlineCompletionErrors.CommentTooLong, result.Error);
    }

    [Fact]
    public async Task Un_vehicule_inconnu_ou_sorti_du_parc_est_refuse()
    {
        var deadline = await AddOneOffAsync();
        var unknown = await Complete.ExecuteAsync(Guid.NewGuid(), deadline.Id, new DeadlineCompletionInput(Today), CancellationToken.None);
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var retired = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today), CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, unknown.Error);
        Assert.Equal(VehicleErrors.Retired, retired.Error);
        Assert.Empty(_interventions.Saved);
    }

    [Fact]
    public async Task Le_montant_est_transmis_a_l_intervention_creee_et_un_montant_negatif_est_refuse()
    {
        var deadline = await AddOneOffAsync();

        var negative = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today, Amount: -5m), CancellationToken.None);
        var ok = await Complete.ExecuteAsync(_vehicle.Id, deadline.Id, new DeadlineCompletionInput(Today, Amount: 89.9m), CancellationToken.None);

        Assert.Equal(InterventionErrors.AmountNegative, negative.Error);
        Assert.True(ok.IsSuccess);
        Assert.Equal(89.9m, Assert.Single(_interventions.Saved).Amount);
    }
}
