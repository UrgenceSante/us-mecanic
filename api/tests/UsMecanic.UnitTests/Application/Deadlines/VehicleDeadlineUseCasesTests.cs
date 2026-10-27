using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Deadlines;

public class VehicleDeadlineUseCasesTests
{
    private static readonly DateOnly Today = new(2026, 10, 1); // FixedClock.Default

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryDeadlineTypeRepository _types = new();
    private readonly InMemoryDeadlineThresholdsRepository _thresholds = new();
    private readonly InMemoryMileageReadingRepository _readings = new();
    private readonly InMemoryVehicleDeadlineRepository _deadlines = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;
    private readonly DeadlineType _type;

    public VehicleDeadlineUseCasesTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
        _type = _types.Seed("Contrôle technique");
    }

    private VehicleDeadlineDtoFactory Factory => new(_types, _thresholds, _readings, _clock.Calendar);

    private AddVehicleDeadline Add => new(new MileageVehicleGuard(_vehicles), _types, _deadlines, Factory, _clock.Calendar);

    private UpdateVehicleDeadlineInterval Update => new(new MileageVehicleGuard(_vehicles), _deadlines, Factory, _clock.Calendar);

    private ListVehicleDeadlines List => new(new MileageVehicleGuard(_vehicles), _deadlines, Factory);

    private static VehicleDeadlineInput OneOff(Guid typeId, DateOnly? dueDate, int? dueKm = null) =>
        new(typeId, DueDate: dueDate, DueKm: dueKm);

    [Fact]
    public async Task Ajoute_une_echeance_ponctuelle_avec_son_statut_et_l_enregistre()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today.AddDays(10)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Contrôle technique", result.Value.TypeName);
        Assert.Equal(Today.AddDays(10), result.Value.DueDate);
        Assert.Equal(DeadlineStatus.DueSoon, result.Value.Status);
        Assert.False(result.Value.IsClosed);
        Assert.Single(_deadlines.Saved);
        Assert.Equal(1, _deadlines.SaveCount);
    }

    [Fact]
    public async Task Ajoute_une_echeance_periodique_calculee_depuis_la_derniere_realisation()
    {
        var input = new VehicleDeadlineInput(_type.Id, 12, 30_000, Today.AddMonths(-11), 20_000);

        var result = await Add.ExecuteAsync(_vehicle.Id, input, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Today.AddMonths(1), result.Value.DueDate);
        Assert.Equal(50_000, result.Value.DueKm);
        Assert.Equal(Today.AddMonths(-11), result.Value.LastDoneDate);
    }

    [Fact]
    public async Task Le_statut_utilise_le_releve_le_plus_recent_par_date()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-20), 49_500);
        _readings.Seed(_vehicle.Id, Today.AddDays(-40), 40_000);
        var input = new VehicleDeadlineInput(_type.Id, DueKm: 50_000);

        var result = await Add.ExecuteAsync(_vehicle.Id, input, CancellationToken.None);

        Assert.Equal(DeadlineStatus.DueSoon, result.Value.Status); // 500 km restants
    }

    [Fact]
    public async Task Le_statut_est_depasse_quand_le_km_actuel_atteint_l_echeance()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-1), 50_000);

        var result = await Add.ExecuteAsync(_vehicle.Id, new VehicleDeadlineInput(_type.Id, DueKm: 50_000), CancellationToken.None);

        Assert.Equal(DeadlineStatus.Overdue, result.Value.Status);
    }

    [Fact]
    public async Task Sans_releve_le_critere_km_est_ignore()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, new VehicleDeadlineInput(_type.Id, DueKm: 100), CancellationToken.None);

        Assert.Equal(DeadlineStatus.UpToDate, result.Value.Status);
    }

    [Fact]
    public async Task Les_seuils_du_type_l_emportent_sur_les_globaux()
    {
        var wide = DeadlineType.Create("Assurance", new DeadlineTypeSettings(12, null, 90, null)).Value;
        _types.Saved.Add(wide);

        var result = await Add.ExecuteAsync(_vehicle.Id, OneOff(wide.Id, Today.AddDays(60)), CancellationToken.None);

        Assert.Equal(DeadlineStatus.DueSoon, result.Value.Status);
    }

    [Fact]
    public async Task Les_seuils_globaux_modifies_s_appliquent_aux_types_sans_seuil()
    {
        _ = _thresholds.Thresholds.Update(5, 100);

        var result = await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today.AddDays(10)), CancellationToken.None);

        Assert.Equal(DeadlineStatus.UpToDate, result.Value.Status);
    }

    [Fact]
    public async Task Refuse_un_type_inconnu_ou_inactif()
    {
        var inactive = _types.Seed("Ancien", active: false);

        var unknown = await Add.ExecuteAsync(_vehicle.Id, OneOff(Guid.NewGuid(), Today), CancellationToken.None);
        var disabled = await Add.ExecuteAsync(_vehicle.Id, OneOff(inactive.Id, Today), CancellationToken.None);

        Assert.Equal(VehicleDeadlineErrors.TypeUnknown, unknown.Error);
        Assert.Equal(VehicleDeadlineErrors.TypeInactive, disabled.Error);
        Assert.Empty(_deadlines.Saved);
    }

    [Fact]
    public async Task Refuse_un_second_exemplaire_actif_du_meme_type()
    {
        await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today), CancellationToken.None);

        var result = await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today.AddDays(5)), CancellationToken.None);

        Assert.Equal(VehicleDeadlineErrors.AlreadyActive, result.Error);
        Assert.Single(_deadlines.Saved);
    }

    [Fact]
    public async Task Le_meme_type_est_permis_sur_un_autre_vehicule()
    {
        var other = _vehicles.Seed("EF-456-GH");
        await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today), CancellationToken.None);

        var result = await Add.ExecuteAsync(other.Id, OneOff(_type.Id, Today), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refuse_une_echeance_sans_critere()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, null), CancellationToken.None);

        Assert.Equal(VehicleDeadlineErrors.CriterionRequired, result.Error);
        Assert.Empty(_deadlines.Saved);
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu_et_toute_ecriture_hors_parc()
    {
        var unknown = await Add.ExecuteAsync(Guid.NewGuid(), OneOff(_type.Id, Today), CancellationToken.None);
        var added = await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today), CancellationToken.None);
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var add = await Add.ExecuteAsync(_vehicle.Id, OneOff(_types.Seed("Autre").Id, Today), CancellationToken.None);
        var update = await Update.ExecuteAsync(_vehicle.Id, added.Value.Id, new VehicleDeadlineIntervalInput(6, null), CancellationToken.None);
        var list = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, unknown.Error);
        Assert.Equal(VehicleErrors.Retired, add.Error);
        Assert.Equal(VehicleErrors.Retired, update.Error);
        Assert.True(list.IsSuccess);
        Assert.Single(list.Value);
    }

    [Fact]
    public async Task Modifie_la_periodicite_et_recalcule()
    {
        var input = new VehicleDeadlineInput(_type.Id, 12, null, Today.AddMonths(-3), null);
        var added = await Add.ExecuteAsync(_vehicle.Id, input, CancellationToken.None);

        var result = await Update.ExecuteAsync(_vehicle.Id, added.Value.Id, new VehicleDeadlineIntervalInput(6, 10_000), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value.IntervalMonths);
        Assert.Equal(Today.AddMonths(3), result.Value.DueDate);
        Assert.Null(result.Value.DueKm); // pas de km de dernière réalisation
        Assert.Equal(2, _deadlines.SaveCount);
    }

    [Fact]
    public async Task Modification_d_une_echeance_inconnue_ou_invalide()
    {
        var added = await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today), CancellationToken.None);

        var unknown = await Update.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), new VehicleDeadlineIntervalInput(6, null), CancellationToken.None);
        var invalid = await Update.ExecuteAsync(_vehicle.Id, added.Value.Id, new VehicleDeadlineIntervalInput(-1, null), CancellationToken.None);

        Assert.Equal(VehicleDeadlineErrors.NotFound, unknown.Error);
        Assert.Equal(VehicleDeadlineErrors.IntervalMonthsInvalid, invalid.Error);
        Assert.Equal(1, _deadlines.SaveCount);
    }

    [Fact]
    public async Task Liste_les_echeances_les_plus_urgentes_d_abord()
    {
        var insurance = _types.Seed("Assurance");
        var extinguisher = _types.Seed("Extincteur");
        await Add.ExecuteAsync(_vehicle.Id, OneOff(_type.Id, Today.AddDays(200)), CancellationToken.None);
        await Add.ExecuteAsync(_vehicle.Id, OneOff(insurance.Id, Today.AddDays(-2)), CancellationToken.None);
        await Add.ExecuteAsync(_vehicle.Id, OneOff(extinguisher.Id, Today.AddDays(10)), CancellationToken.None);

        var result = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        IReadOnlyList<DeadlineStatus> expected = [DeadlineStatus.Overdue, DeadlineStatus.DueSoon, DeadlineStatus.UpToDate];
        Assert.Equal(expected, result.Value.Select(d => d.Status!.Value));
        Assert.Equal("Assurance", result.Value[0].TypeName);
    }
}
