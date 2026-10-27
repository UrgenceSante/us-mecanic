using UsMecanic.Application.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

public class VehicleCurrentMileageTests
{
    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryMileageReadingRepository _readings = new();
    private readonly VehicleTypeLookup _lookup;

    public VehicleCurrentMileageTests() => _lookup = new VehicleTypeLookup(new InMemoryVehicleTypeRepository(), _readings);

    [Fact]
    public async Task La_fiche_expose_le_km_et_la_date_du_releve_le_plus_recent()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        _readings.Seed(vehicle.Id, new DateOnly(2026, 9, 1), 20_000);
        _readings.Seed(vehicle.Id, new DateOnly(2026, 9, 20), 21_500);

        var result = await new GetVehicle(_vehicles, _lookup).ExecuteAsync(vehicle.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(21_500, result.Value.CurrentMileage);
        Assert.Equal(new DateOnly(2026, 9, 20), result.Value.CurrentMileageDate);
    }

    [Fact]
    public async Task Un_vehicule_sans_releve_a_des_champs_vides()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");

        var result = await new GetVehicle(_vehicles, _lookup).ExecuteAsync(vehicle.Id, CancellationToken.None);

        Assert.Null(result.Value.CurrentMileage);
        Assert.Null(result.Value.CurrentMileageDate);
    }

    [Fact]
    public async Task La_liste_charge_les_km_actuels_en_une_seule_requete()
    {
        var first = _vehicles.Seed("AB-123-CD");
        var second = _vehicles.Seed("EF-456-GH");
        var third = _vehicles.Seed("IJ-789-KL");
        _readings.Seed(first.Id, new DateOnly(2026, 9, 1), 10_000);
        _readings.Seed(first.Id, new DateOnly(2026, 9, 10), 10_400);
        _readings.Seed(second.Id, new DateOnly(2026, 8, 1), 55_000);

        var list = await new ListVehicles(_vehicles, _lookup).ExecuteAsync(new VehicleFilter(), CancellationToken.None);

        Assert.Equal(1, _readings.CurrentByVehicleCalls);
        Assert.Equal(10_400, list.Single(v => v.Id == first.Id).CurrentMileage);
        Assert.Equal(new DateOnly(2026, 9, 10), list.Single(v => v.Id == first.Id).CurrentMileageDate);
        Assert.Equal(55_000, list.Single(v => v.Id == second.Id).CurrentMileage);
        Assert.Null(list.Single(v => v.Id == third.Id).CurrentMileage);
        Assert.Null(list.Single(v => v.Id == third.Id).CurrentMileageDate);
    }

    [Fact]
    public async Task Le_km_actuel_suit_la_suppression_du_releve_le_plus_recent()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        _readings.Seed(vehicle.Id, new DateOnly(2026, 9, 1), 20_000);
        var latest = _readings.Seed(vehicle.Id, new DateOnly(2026, 9, 20), 21_500);

        _readings.Remove(latest);
        var result = await new GetVehicle(_vehicles, _lookup).ExecuteAsync(vehicle.Id, CancellationToken.None);

        Assert.Equal(20_000, result.Value.CurrentMileage);
    }
}
