using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Deadlines;

public class AssignDeadlineTypeTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryVehicleTypeRepository _vehicleTypes = new();
    private readonly InMemoryDeadlineTypeRepository _types = new();
    private readonly InMemoryMileageReadingRepository _readings = new();
    private readonly InMemoryVehicleDeadlineRepository _deadlines = new();
    private readonly FixedClock _clock = new();
    private readonly VehicleType _ambulance;

    public AssignDeadlineTypeTests()
    {
        _ambulance = _vehicleTypes.Seed("Ambulance");
    }

    private AssignDeadlineType UseCase => new(_types, _vehicleTypes, _vehicles, _deadlines, _readings, _clock.Calendar);

    private Vehicle SeedVehicle(string plate, VehicleType? type = null) =>
        _vehicles.Seed(plate, InMemoryVehicleRepository.EmptyDetails with { TypeId = (type ?? _ambulance).Id });

    private DeadlineType SeedType(int? months, int? km, bool active = true)
    {
        var type = DeadlineType.Create($"Type {Guid.NewGuid():N}", new DeadlineTypeSettings(months, km, null, null)).Value;
        if (!active)
        {
            type.Deactivate();
        }

        _types.Saved.Add(type);
        return type;
    }

    [Fact]
    public async Task Ajoute_l_echeance_aux_vehicules_du_type_avec_la_periodicite_par_defaut_et_aujourd_hui()
    {
        var first = SeedVehicle("AB-123-CD");
        SeedVehicle("EF-456-GH");
        var type = SeedType(12, null);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Added);
        Assert.Empty(result.Value.Errors);
        Assert.Equal(2, _deadlines.Saved.Count);
        Assert.Equal(1, _deadlines.SaveCount);
        var deadline = _deadlines.Saved.Single(d => d.VehicleId == first.Id);
        Assert.Equal(12, deadline.IntervalMonths);
        Assert.Equal(Today, deadline.LastDoneDate);
        Assert.Equal(Today.AddMonths(12), deadline.DueDate);
    }

    [Fact]
    public async Task Utilise_la_date_de_derniere_realisation_fournie()
    {
        SeedVehicle("AB-123-CD");
        var type = SeedType(12, null);
        var done = Today.AddMonths(-6);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], done, CancellationToken.None);

        Assert.Equal(1, result.Value.Added);
        Assert.Equal(done.AddMonths(12), _deadlines.Saved.Single().DueDate);
    }

    [Fact]
    public async Task Utilise_le_km_actuel_quand_le_type_a_une_periodicite_en_km()
    {
        var vehicle = SeedVehicle("AB-123-CD");
        _readings.Seed(vehicle.Id, Today.AddDays(-2), 40_000);
        var type = SeedType(null, 10_000);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], null, CancellationToken.None);

        Assert.Equal(1, result.Value.Added);
        var deadline = _deadlines.Saved.Single();
        Assert.Equal(40_000, deadline.LastDoneKm);
        Assert.Equal(50_000, deadline.DueKm);
    }

    [Fact]
    public async Task Un_vehicule_sans_releve_est_en_erreur_quand_le_type_a_un_critere_km()
    {
        var withReading = SeedVehicle("AB-123-CD");
        var without = SeedVehicle("EF-456-GH");
        _readings.Seed(withReading.Id, Today.AddDays(-2), 40_000);
        var type = SeedType(null, 10_000);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], null, CancellationToken.None);

        Assert.Equal(1, result.Value.Added);
        var error = Assert.Single(result.Value.Errors);
        Assert.Equal(without.Id, error.VehicleId);
        Assert.Equal(DeadlineCompletionErrors.KilometersRequired.Code, error.Code);
    }

    [Fact]
    public async Task Ignore_les_vehicules_qui_ont_deja_ce_type_en_cours()
    {
        var active = SeedVehicle("AB-123-CD");
        SeedVehicle("EF-456-GH");
        var type = SeedType(12, null);
        _deadlines.Saved.Add(VehicleDeadline.Create(active.Id, type, new VehicleDeadlineSchedule(12, null, Today, null, null, null), Today).Value);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], null, CancellationToken.None);

        Assert.Equal(1, result.Value.Added);
        Assert.Equal(1, result.Value.SkippedAlreadyActive);
        Assert.Equal(2, _deadlines.Saved.Count);
    }

    [Fact]
    public async Task Ignore_les_vehicules_sortis_du_parc_et_garde_les_immobilises()
    {
        SeedVehicle("AB-123-CD").ChangeStatus(VehicleStatus.Retired, FixedClock.Default);
        SeedVehicle("EF-456-GH").ChangeStatus(VehicleStatus.Immobilized, FixedClock.Default);
        var type = SeedType(12, null);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], null, CancellationToken.None);

        Assert.Equal(1, result.Value.Added);
        Assert.Equal(1, result.Value.SkippedRetired);
    }

    [Fact]
    public async Task Ne_touche_pas_aux_vehicules_des_autres_types_et_accepte_plusieurs_types()
    {
        var vsl = _vehicleTypes.Seed("VSL");
        var other = _vehicleTypes.Seed("Autre");
        SeedVehicle("AB-123-CD");
        SeedVehicle("EF-456-GH", vsl);
        SeedVehicle("IJ-789-KL", other);
        var type = SeedType(12, null);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id, vsl.Id, _ambulance.Id], null, CancellationToken.None);

        Assert.Equal(2, result.Value.Added);
    }

    [Fact]
    public async Task Refuse_un_type_d_echeance_inactif_ponctuel_ou_inconnu()
    {
        var inactive = SeedType(12, null, active: false);
        var oneOff = SeedType(null, null);

        Assert.Equal(VehicleDeadlineErrors.TypeInactive, (await UseCase.ExecuteAsync(inactive.Id, [_ambulance.Id], null, CancellationToken.None)).Error);
        Assert.Equal(VehicleDeadlineErrors.OneOffNotAssignable, (await UseCase.ExecuteAsync(oneOff.Id, [_ambulance.Id], null, CancellationToken.None)).Error);
        Assert.Equal(DeadlineTypeErrors.NotFound, (await UseCase.ExecuteAsync(Guid.NewGuid(), [_ambulance.Id], null, CancellationToken.None)).Error);
    }

    [Fact]
    public async Task Refuse_des_types_de_vehicule_absents_ou_inconnus_et_une_date_future()
    {
        var type = SeedType(12, null);

        Assert.Equal(VehicleDeadlineErrors.VehicleTypesRequired, (await UseCase.ExecuteAsync(type.Id, [], null, CancellationToken.None)).Error);
        Assert.Equal(VehicleDeadlineErrors.VehicleTypesRequired, (await UseCase.ExecuteAsync(type.Id, null, null, CancellationToken.None)).Error);
        Assert.Equal(VehicleTypeErrors.Unknown, (await UseCase.ExecuteAsync(type.Id, [Guid.NewGuid()], null, CancellationToken.None)).Error);
        Assert.Equal(VehicleDeadlineErrors.LastDoneInFuture, (await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], Today.AddDays(1), CancellationToken.None)).Error);
        Assert.Empty(_deadlines.Saved);
    }

    [Fact]
    public async Task N_enregistre_rien_quand_aucun_vehicule_n_est_ajoute()
    {
        var type = SeedType(12, null);

        var result = await UseCase.ExecuteAsync(type.Id, [_ambulance.Id], null, CancellationToken.None);

        Assert.Equal(0, result.Value.Added);
        Assert.Equal(0, _deadlines.SaveCount);
    }
}
