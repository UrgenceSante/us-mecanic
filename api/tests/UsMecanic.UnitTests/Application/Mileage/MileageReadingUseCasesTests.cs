using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Mileage;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Mileage;

public class MileageReadingUseCasesTests
{
    private static readonly DateOnly Today = new(2026, 10, 1); // FixedClock.Default

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryMileageReadingRepository _readings = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;

    public MileageReadingUseCasesTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
    }

    private MileageVehicleGuard Guard => new(_vehicles);

    private AddMileageReading Add => new(Guard, _readings, _clock, _clock.Calendar);

    private UpdateMileageReading Update => new(Guard, _readings, _clock, _clock.Calendar);

    private DeleteMileageReading Delete => new(Guard, _readings);

    private ListMileageReadings List => new(Guard, _readings);

    private void Retire() => _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

    [Fact]
    public async Task Ajoute_un_releve_et_l_enregistre()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, new MileageReadingInput(Today, 12_000), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(12_000, result.Value.Kilometers);
        Assert.Equal(_vehicle.Id, result.Value.VehicleId);
        Assert.Equal(_clock.Now, result.Value.RecordedAt);
        Assert.Single(_readings.Saved);
        Assert.Equal(1, _readings.SaveCount);
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu()
    {
        var result = await Add.ExecuteAsync(Guid.NewGuid(), new MileageReadingInput(Today, 1), CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
        Assert.Empty(_readings.Saved);
    }

    [Fact]
    public async Task Refuse_toute_ecriture_sur_un_vehicule_sorti_du_parc()
    {
        var reading = _readings.Seed(_vehicle.Id, Today.AddDays(-5), 1_000);
        Retire();
        var input = new MileageReadingInput(Today, 2_000);

        var added = await Add.ExecuteAsync(_vehicle.Id, input, CancellationToken.None);
        var updated = await Update.ExecuteAsync(_vehicle.Id, reading.Id, input, CancellationToken.None);
        var deleted = await Delete.ExecuteAsync(_vehicle.Id, reading.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.Retired, added.Error);
        Assert.Equal(VehicleErrors.Retired, updated.Error);
        Assert.Equal(VehicleErrors.Retired, deleted.Error);
        Assert.Single(_readings.Saved);
        Assert.Equal(1_000, reading.Kilometers);
    }

    [Fact]
    public async Task L_historique_d_un_vehicule_sorti_du_parc_reste_consultable()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-5), 1_000);
        Retire();

        var result = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Liste_du_plus_recent_au_plus_ancien_et_uniquement_pour_le_vehicule()
    {
        var other = _vehicles.Seed("EF-456-GH");
        _readings.Seed(_vehicle.Id, Today.AddDays(-10), 1_000);
        _readings.Seed(_vehicle.Id, Today.AddDays(-1), 1_500);
        _readings.Seed(_vehicle.Id, Today.AddDays(-5), 1_200);
        _readings.Seed(other.Id, Today, 99_000);

        var result = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal([1_500, 1_200, 1_000], result.Value.Select(r => r.Kilometers));
    }

    [Fact]
    public async Task Lister_pour_un_vehicule_inconnu_renvoie_introuvable()
    {
        var result = await List.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task L_ajout_applique_les_controles_de_coherence()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-2), 10_000);

        var lower = await Add.ExecuteAsync(_vehicle.Id, new MileageReadingInput(Today, 9_000), CancellationToken.None);
        var fast = await Add.ExecuteAsync(_vehicle.Id, new MileageReadingInput(Today, 13_000), CancellationToken.None);
        var confirmed = await Add.ExecuteAsync(_vehicle.Id, new MileageReadingInput(Today, 13_000, ConfirmHighMileage: true), CancellationToken.None);

        Assert.Equal(MileageReadingErrors.ReasonRequired, lower.Error);
        Assert.Equal(MileageReadingErrors.HighMileageConfirmationRequired, fast.Error);
        Assert.True(confirmed.IsSuccess);
        Assert.Equal(2, _readings.Saved.Count);
    }

    [Fact]
    public async Task Modifie_un_releve_sans_le_comparer_a_lui_meme()
    {
        _readings.Seed(_vehicle.Id, Today.AddDays(-10), 5_000);
        var middle = _readings.Seed(_vehicle.Id, Today.AddDays(-5), 5_300);
        _readings.Seed(_vehicle.Id, Today.AddDays(-1), 5_600);

        var result = await Update.ExecuteAsync(_vehicle.Id, middle.Id, new MileageReadingInput(Today.AddDays(-5), 5_100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5_100, middle.Kilometers);
        Assert.Equal(1, _readings.SaveCount);
    }

    [Fact]
    public async Task Une_modification_refusee_ne_change_rien()
    {
        var first = _readings.Seed(_vehicle.Id, Today.AddDays(-10), 5_000);
        var second = _readings.Seed(_vehicle.Id, Today.AddDays(-5), 5_300);

        var result = await Update.ExecuteAsync(_vehicle.Id, second.Id, new MileageReadingInput(Today.AddDays(-5), 4_000), CancellationToken.None);

        Assert.Equal(MileageReadingErrors.ReasonRequired, result.Error);
        Assert.Equal(5_300, second.Kilometers);
        Assert.Equal(5_000, first.Kilometers);
        Assert.Equal(0, _readings.SaveCount);
    }

    [Fact]
    public async Task Modifier_ou_supprimer_un_releve_inconnu_ou_d_un_autre_vehicule_renvoie_introuvable()
    {
        var other = _vehicles.Seed("EF-456-GH");
        var foreign = _readings.Seed(other.Id, Today.AddDays(-5), 1_000);
        var input = new MileageReadingInput(Today, 2_000);

        var unknownUpdate = await Update.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), input, CancellationToken.None);
        var foreignUpdate = await Update.ExecuteAsync(_vehicle.Id, foreign.Id, input, CancellationToken.None);
        var foreignDelete = await Delete.ExecuteAsync(_vehicle.Id, foreign.Id, CancellationToken.None);

        Assert.Equal(MileageReadingErrors.NotFound, unknownUpdate.Error);
        Assert.Equal(MileageReadingErrors.NotFound, foreignUpdate.Error);
        Assert.Equal(MileageReadingErrors.NotFound, foreignDelete.Error);
        Assert.Single(_readings.Saved);
    }

    [Fact]
    public async Task Supprime_un_releve()
    {
        var reading = _readings.Seed(_vehicle.Id, Today.AddDays(-5), 1_000);

        var result = await Delete.ExecuteAsync(_vehicle.Id, reading.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_readings.Saved);
        Assert.Equal(1, _readings.SaveCount);
    }
}
