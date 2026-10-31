using System.Globalization;
using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Mileage;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Maintenance;

public class InterventionUseCasesTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryDeadlineTypeRepository _types = new();
    private readonly InMemoryDeadlineThresholdsRepository _thresholds = new();
    private readonly InMemoryMileageReadingRepository _readings = new();
    private readonly InMemoryVehicleDeadlineRepository _deadlines = new();
    private readonly InMemoryGarageRepository _garages = new();
    private readonly InMemoryInterventionRepository _interventions = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;
    private readonly DeadlineType _inspection;
    private readonly DeadlineType _maintenance;

    public InterventionUseCasesTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
        _inspection = _types.Seed("Contrôle technique");
        _maintenance = _types.Seed("Entretien");
        _interventions.SharedSaves.Add(() => _deadlines.SaveChangesAsync(CancellationToken.None));
        _interventions.SharedSaves.Add(() => _readings.SaveChangesAsync(CancellationToken.None));
    }

    private AddIntervention Add => new(
        new MileageVehicleGuard(_vehicles), _garages, _deadlines, _readings, _interventions, DtoFactory, _clock, _clock.Calendar);

    private ListInterventions List => new(new MileageVehicleGuard(_vehicles), _interventions, DtoFactory);

    private DeleteIntervention Delete => new(new MileageVehicleGuard(_vehicles), _interventions);

    private InterventionDtoFactory DtoFactory => new(_garages, _deadlines, _types);

    private async Task<VehicleDeadlineDto> AddDeadlineAsync(Guid vehicleId, VehicleDeadlineInput input)
    {
        var factory = new VehicleDeadlineDtoFactory(_types, _thresholds, _readings, _clock.Calendar);
        var add = new AddVehicleDeadline(new MileageVehicleGuard(_vehicles), _types, _deadlines, factory, _clock.Calendar);
        return (await add.ExecuteAsync(vehicleId, input, CancellationToken.None)).Value;
    }

    private Task<VehicleDeadlineDto> AddPeriodicKmAsync() =>
        AddDeadlineAsync(_vehicle.Id, new VehicleDeadlineInput(_maintenance.Id, 12, 30_000, Today.AddMonths(-11), 20_000));

    private Task<VehicleDeadlineDto> AddOneOffAsync() =>
        AddDeadlineAsync(_vehicle.Id, new VehicleDeadlineInput(_inspection.Id, DueDate: Today.AddDays(10)));

    private VehicleDeadline Stored(Guid deadlineId) => _deadlines.Saved.Single(d => d.Id == deadlineId);

    private Task<Result<InterventionDto>> ExecuteAsync(InterventionInput input) =>
        Add.ExecuteAsync(_vehicle.Id, input, CancellationToken.None);

    [Fact]
    public async Task Une_echeance_periodique_cochee_est_realisee_et_le_km_cree_un_releve()
    {
        var deadline = await AddPeriodicKmAsync();
        var garage = _garages.Seed("Garage Martin");
        var date = Today.AddDays(-3);

        var result = await ExecuteAsync(new InterventionInput(date, 21_000, garage.Id, " Vidange ", [deadline.Id]));

        Assert.True(result.IsSuccess);
        Assert.Equal("Vidange", result.Value.Work);
        Assert.Equal(date, result.Value.Date);
        Assert.Equal(21_000, result.Value.Kilometers);
        Assert.Equal(new InterventionGarageDto(garage.Id, "Garage Martin"), result.Value.Garage);
        Assert.Equal(new InterventionDeadlineDto(deadline.Id, "Entretien"), Assert.Single(result.Value.Deadlines));
        Assert.Equal(_clock.Now, result.Value.RecordedAt);
        var done = Stored(deadline.Id);
        Assert.Equal(date, done.LastDoneDate);
        Assert.Equal(21_000, done.LastDoneKm);
        Assert.Equal(date.AddMonths(12), done.DueDate);
        Assert.Equal(51_000, done.DueKm);
        Assert.False(done.IsClosed);
        var reading = Assert.Single(_readings.Saved);
        Assert.Equal((date, 21_000), (reading.Date, reading.Kilometers));
        Assert.Single(_interventions.Saved);
        Assert.Equal(1, _interventions.SaveCount);
    }

    [Fact]
    public async Task Une_intervention_imprevue_sans_echeance_ni_km_ne_cree_pas_de_releve()
    {
        var result = await ExecuteAsync(new InterventionInput(Today, Work: "Remplacement pare-brise"));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Deadlines);
        Assert.Null(result.Value.Garage);
        Assert.Null(result.Value.Kilometers);
        Assert.Empty(_readings.Saved);
        Assert.Single(_interventions.Saved);
    }

    [Fact]
    public async Task Plusieurs_echeances_sont_realisees_ensemble_et_la_ponctuelle_est_close()
    {
        var periodic = await AddPeriodicKmAsync();
        var oneOff = await AddOneOffAsync();

        var result = await ExecuteAsync(new InterventionInput(Today, 25_000, Work: "Entretien et contrôle", DeadlineIds: [periodic.Id, oneOff.Id]));

        Assert.True(result.IsSuccess);
        string?[] expected = ["Contrôle technique", "Entretien"];
        Assert.Equal(expected, result.Value.Deadlines.Select(d => d.TypeName).ToArray());
        Assert.True(Stored(oneOff.Id).IsClosed);
        Assert.False(Stored(periodic.Id).IsClosed);
        Assert.Single(_readings.Saved);
    }

    [Fact]
    public async Task Le_km_est_obligatoire_si_une_echeance_cochee_a_un_critere_km_et_rien_n_est_ecrit()
    {
        var oneOff = await AddOneOffAsync();
        var periodic = await AddPeriodicKmAsync();

        var result = await ExecuteAsync(new InterventionInput(Today, Work: "Entretien", DeadlineIds: [oneOff.Id, periodic.Id]));

        Assert.Equal(DeadlineCompletionErrors.KilometersRequired, result.Error);
        Assert.False(Stored(oneOff.Id).IsClosed);
        Assert.Equal(Today.AddMonths(-11), Stored(periodic.Id).LastDoneDate);
        Assert.Empty(_interventions.Saved);
        Assert.Equal(0, _interventions.SaveCount);
        Assert.Empty(_readings.Saved);
    }

    [Fact]
    public async Task Une_echeance_inconnue_ou_d_un_autre_vehicule_est_refusee()
    {
        var other = _vehicles.Seed("EF-456-GH");
        var foreign = await AddDeadlineAsync(other.Id, new VehicleDeadlineInput(_inspection.Id, DueDate: Today.AddDays(10)));

        var ofOtherVehicle = await ExecuteAsync(new InterventionInput(Today, Work: "Contrôle", DeadlineIds: [foreign.Id]));
        var unknown = await ExecuteAsync(new InterventionInput(Today, Work: "Contrôle", DeadlineIds: [Guid.NewGuid()]));

        Assert.Equal(InterventionErrors.DeadlineUnknown, ofOtherVehicle.Error);
        Assert.Equal(InterventionErrors.DeadlineUnknown, unknown.Error);
        Assert.False(Stored(foreign.Id).IsClosed);
        Assert.Empty(_interventions.Saved);
    }

    [Fact]
    public async Task Une_echeance_close_est_refusee()
    {
        var oneOff = await AddOneOffAsync();
        var first = await ExecuteAsync(new InterventionInput(Today, Work: "Contrôle", DeadlineIds: [oneOff.Id]));

        var again = await ExecuteAsync(new InterventionInput(Today, Work: "Contrôle bis", DeadlineIds: [oneOff.Id]));

        Assert.True(first.IsSuccess);
        Assert.Equal(InterventionErrors.DeadlineClosed, again.Error);
        Assert.Single(_interventions.Saved);
    }

    [Fact]
    public async Task Travaux_date_future_et_doublons_sont_refuses()
    {
        var oneOff = await AddOneOffAsync();

        var noWork = await ExecuteAsync(new InterventionInput(Today, Work: " "));
        var future = await ExecuteAsync(new InterventionInput(Today.AddDays(1), Work: "Vidange"));
        var duplicate = await ExecuteAsync(new InterventionInput(Today, Work: "Contrôle", DeadlineIds: [oneOff.Id, oneOff.Id]));

        Assert.Equal(InterventionErrors.WorkRequired, noWork.Error);
        Assert.Equal(InterventionErrors.DateInFuture, future.Error);
        Assert.Equal(InterventionErrors.DeadlineDuplicated, duplicate.Error);
        Assert.Empty(_interventions.Saved);
        Assert.False(Stored(oneOff.Id).IsClosed);
    }

    [Fact]
    public async Task Un_garage_inconnu_ou_inactif_est_refuse()
    {
        var inactive = _garages.Seed("Garage fermé", active: false);

        var unknown = await ExecuteAsync(new InterventionInput(Today, GarageId: Guid.NewGuid(), Work: "Vidange"));
        var closed = await ExecuteAsync(new InterventionInput(Today, GarageId: inactive.Id, Work: "Vidange"));

        Assert.Equal(InterventionErrors.GarageUnknown, unknown.Error);
        Assert.Equal(InterventionErrors.GarageInactive, closed.Error);
        Assert.Empty(_interventions.Saved);
    }

    [Fact]
    public async Task Un_km_incoherent_est_refuse_sans_motif_puis_accepte_avec_motif()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-30), 30_000);
        var deadline = await AddDeadlineAsync(_vehicle.Id, new VehicleDeadlineInput(_maintenance.Id, 12, null, Today.AddMonths(-11)));

        var refused = await ExecuteAsync(new InterventionInput(Today.AddDays(-5), 25_000, Work: "Vidange", DeadlineIds: [deadline.Id]));
        var accepted = await ExecuteAsync(new InterventionInput(Today.AddDays(-5), 25_000, Work: "Vidange", DeadlineIds: [deadline.Id], Reason: "Compteur remplacé"));

        Assert.Equal(MileageReadingErrors.ReasonRequired, refused.Error);
        Assert.True(accepted.IsSuccess);
        Assert.Equal(2, _readings.Saved.Count);
        Assert.Single(_interventions.Saved);
    }

    [Fact]
    public async Task Une_moyenne_superieure_a_1000_km_par_jour_exige_une_confirmation()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-2), 10_000);
        var input = new InterventionInput(Today, 20_000, Work: "Vidange");

        var refused = await ExecuteAsync(input);
        var confirmed = await ExecuteAsync(input with { ConfirmHighMileage = true });

        Assert.Equal(MileageReadingErrors.HighMileageConfirmationRequired, refused.Error);
        Assert.True(confirmed.IsSuccess);
    }

    [Fact]
    public async Task Un_vehicule_inconnu_ou_sorti_du_parc_est_refuse_en_ecriture()
    {
        var unknown = await Add.ExecuteAsync(Guid.NewGuid(), new InterventionInput(Today, Work: "Vidange"), CancellationToken.None);
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var retired = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange"));

        Assert.Equal(VehicleErrors.NotFound, unknown.Error);
        Assert.Equal(VehicleErrors.Retired, retired.Error);
        Assert.Empty(_interventions.Saved);
    }

    [Fact]
    public async Task Le_carnet_est_trie_du_plus_recent_au_plus_ancien_et_reste_lisible_pour_un_vehicule_sorti_du_parc()
    {
        var garage = _garages.Seed("Garage Martin");
        var oneOff = await AddOneOffAsync();
        await ExecuteAsync(new InterventionInput(Today.AddDays(-10), Work: "Pneus", GarageId: garage.Id));
        await ExecuteAsync(new InterventionInput(Today.AddDays(-1), Work: "Contrôle", DeadlineIds: [oneOff.Id]));
        await ExecuteAsync(new InterventionInput(Today.AddDays(-5), Work: "Freins"));
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var result = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        string[] expected = ["Contrôle", "Freins", "Pneus"];
        Assert.Equal(expected, result.Value.Select(i => i.Work).ToArray());
        Assert.Equal("Contrôle technique", Assert.Single(result.Value[0].Deadlines).TypeName);
        Assert.Equal("Garage Martin", result.Value[2].Garage?.Name);
        Assert.Equal(1, _interventions.ListCalls);
    }

    [Fact]
    public async Task Le_carnet_d_un_vehicule_inconnu_ou_vide()
    {
        var unknown = await List.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);
        var empty = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, unknown.Error);
        Assert.True(empty.IsSuccess);
        Assert.Empty(empty.Value);
    }

    [Fact]
    public async Task La_suppression_retire_l_intervention_sans_recalculer_les_echeances_ni_toucher_au_releve()
    {
        var deadline = await AddPeriodicKmAsync();
        var date = Today.AddDays(-3);
        var added = await ExecuteAsync(new InterventionInput(date, 21_000, Work: "Vidange", DeadlineIds: [deadline.Id]));

        var result = await Delete.ExecuteAsync(_vehicle.Id, added.Value.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(added.Value.Id, result.Value);
        Assert.Empty(_interventions.Saved);
        Assert.Single(_readings.Saved);
        var kept = Stored(deadline.Id);
        Assert.Equal(date, kept.LastDoneDate);
        Assert.Equal(21_000, kept.LastDoneKm);
    }

    [Fact]
    public async Task La_suppression_d_une_intervention_inconnue_ou_d_un_autre_vehicule_est_refusee()
    {
        var other = _vehicles.Seed("ZZ-999-ZZ");
        var added = await ExecuteAsync(new InterventionInput(Today.AddDays(-1), Work: "Vidange"));

        var unknown = await Delete.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), CancellationToken.None);
        var otherVehicle = await Delete.ExecuteAsync(other.Id, added.Value.Id, CancellationToken.None);
        var unknownVehicle = await Delete.ExecuteAsync(Guid.NewGuid(), added.Value.Id, CancellationToken.None);

        Assert.Equal(InterventionErrors.NotFound, unknown.Error);
        Assert.Equal(InterventionErrors.NotFound, otherVehicle.Error);
        Assert.Equal(VehicleErrors.NotFound, unknownVehicle.Error);
        Assert.Single(_interventions.Saved);
    }

    [Fact]
    public async Task La_suppression_est_refusee_pour_un_vehicule_sorti_du_parc()
    {
        var added = await ExecuteAsync(new InterventionInput(Today.AddDays(-1), Work: "Vidange"));
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var result = await Delete.ExecuteAsync(_vehicle.Id, added.Value.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.Retired, result.Error);
        Assert.Single(_interventions.Saved);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("120.50")]
    public async Task Le_montant_facultatif_est_enregistre_et_renvoye_a_l_identique(string? amount)
    {
        decimal? expected = amount is null ? null : decimal.Parse(amount, CultureInfo.InvariantCulture);

        var result = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange", Amount: expected));

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Amount);
        Assert.Equal(expected, Assert.Single(_interventions.Saved).Amount);
    }

    [Fact]
    public async Task Un_montant_negatif_ou_trop_precis_est_refuse_sans_rien_enregistrer()
    {
        var negative = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange", Amount: -1m));
        var tooPrecise = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange", Amount: 1.234m));

        Assert.Equal(InterventionErrors.AmountNegative, negative.Error);
        Assert.Equal(InterventionErrors.AmountTooPrecise, tooPrecise.Error);
        Assert.Empty(_interventions.Saved);
    }

    private ChangeInterventionAmount ChangeAmount => new(new MileageVehicleGuard(_vehicles), _interventions, DtoFactory);

    [Fact]
    public async Task Le_montant_d_une_intervention_est_modifie_puis_vide()
    {
        var added = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange", Amount: 10m));

        var changed = await ChangeAmount.ExecuteAsync(_vehicle.Id, added.Value.Id, 99.90m, CancellationToken.None);
        var cleared = await ChangeAmount.ExecuteAsync(_vehicle.Id, added.Value.Id, null, CancellationToken.None);

        Assert.Equal(99.90m, changed.Value.Amount);
        Assert.Equal(added.Value.Id, changed.Value.Id);
        Assert.Null(cleared.Value.Amount);
        Assert.Null(Assert.Single(_interventions.Saved).Amount);
    }

    [Fact]
    public async Task Un_montant_invalide_est_refuse_sans_rien_enregistrer()
    {
        var added = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange", Amount: 10m));
        var saves = _interventions.SaveCount;

        var result = await ChangeAmount.ExecuteAsync(_vehicle.Id, added.Value.Id, -1m, CancellationToken.None);

        Assert.Equal(InterventionErrors.AmountNegative, result.Error);
        Assert.Equal(saves, _interventions.SaveCount);
        Assert.Equal(10m, Assert.Single(_interventions.Saved).Amount);
    }

    [Fact]
    public async Task La_modification_du_montant_d_une_intervention_inconnue_ou_d_un_autre_vehicule_est_refusee()
    {
        var other = _vehicles.Seed("ZZ-999-ZZ");
        var added = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange"));

        var unknown = await ChangeAmount.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), 1m, CancellationToken.None);
        var otherVehicle = await ChangeAmount.ExecuteAsync(other.Id, added.Value.Id, 1m, CancellationToken.None);
        var unknownVehicle = await ChangeAmount.ExecuteAsync(Guid.NewGuid(), added.Value.Id, 1m, CancellationToken.None);

        Assert.Equal(InterventionErrors.NotFound, unknown.Error);
        Assert.Equal(InterventionErrors.NotFound, otherVehicle.Error);
        Assert.Equal(VehicleErrors.NotFound, unknownVehicle.Error);
        Assert.Null(Assert.Single(_interventions.Saved).Amount);
    }

    [Fact]
    public async Task Le_montant_est_refuse_pour_un_vehicule_sorti_du_parc()
    {
        var added = await ExecuteAsync(new InterventionInput(Today, Work: "Vidange"));
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var result = await ChangeAmount.ExecuteAsync(_vehicle.Id, added.Value.Id, 42m, CancellationToken.None);

        Assert.Equal(VehicleErrors.Retired, result.Error);
        Assert.Null(Assert.Single(_interventions.Saved).Amount);
    }
}
