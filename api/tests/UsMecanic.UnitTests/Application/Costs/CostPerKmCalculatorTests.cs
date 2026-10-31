using UsMecanic.Application.Costs.Summary;
using UsMecanic.Domain.Mileage;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Costs;

public class CostPerKmCalculatorTests
{
    private static readonly Guid VehicleA = Guid.NewGuid();
    private static readonly Guid VehicleB = Guid.NewGuid();

    private static MileageReading Reading(Guid vehicleId, string date, int kilometers) =>
        MileageReading.Create(
            vehicleId,
            new MileageReadingEntry(DateOnly.Parse(date, System.Globalization.CultureInfo.InvariantCulture), kilometers, null, ConfirmHighMileage: true),
            [],
            FixedClock.DefaultToday,
            FixedClock.Default).Value;

    private static CostLine Cost(Guid vehicleId, decimal amount, CostFamily family = CostFamily.OneOff) =>
        new(new DateOnly(2026, 3, 1), family, "Test", amount, VehicleId: vehicleId);

    [Fact]
    public void Deux_releves_espaces_de_1000_km_et_250_euros_donnent_0_25_euro_par_km()
    {
        var result = CostPerKmCalculator.Compute(
            [Cost(VehicleA, 100m), Cost(VehicleA, 150m, CostFamily.Loan)],
            [Reading(VehicleA, "2026-01-10", 5000), Reading(VehicleA, "2026-03-10", 6000)],
            singleVehicle: true);

        Assert.True(result.Computable);
        Assert.Equal(1000, result.Kilometers);
        Assert.Equal(250m, result.Cost);
        Assert.Equal(0.25m, result.Value);
        Assert.Equal(0, result.ExcludedVehicles);
    }

    [Fact]
    public void Un_seul_releve_n_est_pas_calculable()
    {
        var result = CostPerKmCalculator.Compute([Cost(VehicleA, 100m)], [Reading(VehicleA, "2026-01-10", 5000)], singleVehicle: true);

        Assert.False(result.Computable);
        Assert.Null(result.Value);
        Assert.Equal(0, result.Kilometers);
    }

    [Fact]
    public void Aucun_releve_n_est_pas_calculable()
    {
        var result = CostPerKmCalculator.Compute([Cost(VehicleA, 100m)], [], singleVehicle: true);

        Assert.False(result.Computable);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Km_parcourus_nuls_ne_sont_pas_calculables()
    {
        var result = CostPerKmCalculator.Compute(
            [Cost(VehicleA, 100m)],
            [Reading(VehicleA, "2026-01-10", 5000), Reading(VehicleA, "2026-02-10", 5000)],
            singleVehicle: true);

        Assert.False(result.Computable);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Periode_sans_depense_avec_deux_releves_donne_zero()
    {
        var result = CostPerKmCalculator.Compute([], [Reading(VehicleA, "2026-01-10", 5000), Reading(VehicleA, "2026-02-10", 5400)], singleVehicle: true);

        Assert.True(result.Computable);
        Assert.Equal(0m, result.Value);
        Assert.Equal(400, result.Kilometers);
    }

    [Fact]
    public void Utilise_le_premier_et_le_dernier_releve_par_date()
    {
        var result = CostPerKmCalculator.Compute(
            [Cost(VehicleA, 90m)],
            [Reading(VehicleA, "2026-02-10", 5300), Reading(VehicleA, "2026-01-10", 5000), Reading(VehicleA, "2026-03-10", 5900)],
            singleVehicle: true);

        Assert.Equal(900, result.Kilometers);
        Assert.Equal(0.1m, result.Value);
    }

    [Fact]
    public void Parc_exclut_et_signale_les_vehicules_non_calculables()
    {
        var result = CostPerKmCalculator.Compute(
            [Cost(VehicleA, 100m), Cost(VehicleB, 900m)],
            [Reading(VehicleA, "2026-01-10", 1000), Reading(VehicleA, "2026-02-10", 1500), Reading(VehicleB, "2026-01-10", 2000)],
            singleVehicle: false);

        Assert.True(result.Computable);
        Assert.Equal(500, result.Kilometers);
        Assert.Equal(100m, result.Cost);
        Assert.Equal(0.2m, result.Value);
        Assert.Equal(1, result.ExcludedVehicles);
    }

    [Fact]
    public void Parc_sans_aucun_vehicule_calculable()
    {
        var result = CostPerKmCalculator.Compute([Cost(VehicleA, 100m)], [Reading(VehicleB, "2026-01-10", 2000)], singleVehicle: false);

        Assert.False(result.Computable);
        Assert.Null(result.Value);
        Assert.Equal(2, result.ExcludedVehicles);
    }

    [Fact]
    public void Arrondit_a_deux_decimales_au_plus_proche()
    {
        var below = CostPerKmCalculator.Compute([Cost(VehicleA, 1m)], [Reading(VehicleA, "2026-01-10", 0), Reading(VehicleA, "2026-02-10", 300)], singleVehicle: true);
        var half = CostPerKmCalculator.Compute([Cost(VehicleA, 1m)], [Reading(VehicleA, "2026-01-10", 0), Reading(VehicleA, "2026-02-10", 200)], singleVehicle: true);

        Assert.Equal(0.00m, below.Value);
        Assert.Equal(0.01m, half.Value);
    }
}
