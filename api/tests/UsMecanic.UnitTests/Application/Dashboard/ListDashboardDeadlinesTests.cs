using UsMecanic.Application.Dashboard;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Dashboard;

public class ListDashboardDeadlinesTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryVehicleTypeRepository _vehicleTypes = new();
    private readonly InMemoryDeadlineTypeRepository _types = new();
    private readonly InMemoryDeadlineThresholdsRepository _thresholds = new();
    private readonly InMemoryMileageReadingRepository _readings = new();
    private readonly InMemoryVehicleDeadlineRepository _deadlines = new();
    private readonly FixedClock _clock = new();

    private ListDashboardDeadlines UseCase => new(_deadlines, _vehicles, _vehicleTypes, _types, _thresholds, _readings, _clock.Calendar);

    private async Task<IEnumerable<Guid>> IdsAsync(DashboardDeadlineFilter? filter = null) =>
        (await UseCase.ExecuteAsync(filter ?? new DashboardDeadlineFilter(), CancellationToken.None)).Select(l => l.Id).ToList();

    private VehicleDeadline AddDeadline(Vehicle vehicle, DeadlineType type, DateOnly? dueDate, int? dueKm = null)
    {
        var deadline = VehicleDeadline.Create(vehicle.Id, type, new VehicleDeadlineSchedule(null, null, null, null, dueDate, dueKm), Today).Value;
        _deadlines.Saved.Add(deadline);
        return deadline;
    }

    [Fact]
    public async Task Les_depassees_viennent_avant_les_proches_puis_par_date_croissante()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var type = _types.Seed("Contrôle technique");
        var soonLater = AddDeadline(vehicle, type, Today.AddDays(20));
        var soonSooner = AddDeadline(vehicle, type, Today.AddDays(5));
        var overdueRecent = AddDeadline(vehicle, type, Today.AddDays(-2));
        var overdueOld = AddDeadline(vehicle, type, Today.AddDays(-30));

        var lines = await UseCase.ExecuteAsync(new DashboardDeadlineFilter(), CancellationToken.None);

        Assert.Equal([overdueOld.Id, overdueRecent.Id, soonSooner.Id, soonLater.Id], lines.Select(l => l.Id));
        Assert.Equal([DeadlineStatus.Overdue, DeadlineStatus.Overdue, DeadlineStatus.DueSoon, DeadlineStatus.DueSoon], lines.Select(l => l.Status));
    }

    [Fact]
    public async Task A_date_egale_le_km_restant_le_plus_faible_passe_en_premier()
    {
        var far = _vehicles.Seed("AB-123-CD");
        var near = _vehicles.Seed("EF-456-GH");
        var type = _types.Seed("Vidange");
        _readings.Seed(far.Id, Today, 40_000);
        _readings.Seed(near.Id, Today, 49_800);
        var farLine = AddDeadline(far, type, Today.AddDays(10), 40_900);
        var nearLine = AddDeadline(near, type, Today.AddDays(10), 50_000);

        var lines = await UseCase.ExecuteAsync(new DashboardDeadlineFilter(), CancellationToken.None);

        Assert.Equal([nearLine.Id, farLine.Id], lines.Select(l => l.Id));
        Assert.Equal(49_800, lines[0].CurrentKm);
    }

    [Fact]
    public async Task Les_echeances_a_jour_et_closes_sont_exclues()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var type = _types.Seed("Assurance");
        AddDeadline(vehicle, type, Today.AddDays(200));
        var closed = AddDeadline(vehicle, type, Today.AddDays(-10));
        closed.Complete(Today, null, Today);

        Assert.Empty(await IdsAsync());
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_est_exclu_et_un_immobilise_est_inclus()
    {
        var retired = _vehicles.Seed("AB-123-CD");
        var immobilized = _vehicles.Seed("EF-456-GH");
        retired.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);
        immobilized.ChangeStatus(VehicleStatus.Immobilized, FixedClock.Default);
        var type = _types.Seed("Contrôle technique");
        AddDeadline(retired, type, Today.AddDays(-1));
        var kept = AddDeadline(immobilized, type, Today.AddDays(-1));

        var lines = await UseCase.ExecuteAsync(new DashboardDeadlineFilter(), CancellationToken.None);

        Assert.Equal([kept.Id], lines.Select(l => l.Id));
        Assert.Equal("EF-456-GH", lines[0].Vehicle.LicensePlate);
    }

    [Fact]
    public async Task Les_filtres_vehicule_type_de_vehicule_type_d_echeance_et_statut_se_combinent()
    {
        var ambulance = _vehicleTypes.Seed("Ambulance");
        var first = _vehicles.Seed("AB-123-CD", new VehicleDetails(null, null, null, null, "AMB-1", ambulance.Id));
        var second = _vehicles.Seed("EF-456-GH");
        var inspection = _types.Seed("Contrôle technique");
        var insurance = _types.Seed("Assurance");
        var firstInspection = AddDeadline(first, inspection, Today.AddDays(-3));
        var firstInsurance = AddDeadline(first, insurance, Today.AddDays(4));
        var secondInspection = AddDeadline(second, inspection, Today.AddDays(-1));

        Assert.Equal([firstInspection.Id, secondInspection.Id, firstInsurance.Id], await IdsAsync());
        Assert.Equal([firstInspection.Id, firstInsurance.Id], await IdsAsync(new DashboardDeadlineFilter(VehicleId: first.Id)));
        Assert.Equal([firstInspection.Id, firstInsurance.Id], await IdsAsync(new DashboardDeadlineFilter(VehicleTypeId: ambulance.Id)));
        Assert.Equal([firstInspection.Id, secondInspection.Id], await IdsAsync(new DashboardDeadlineFilter(DeadlineTypeId: inspection.Id)));
        Assert.Equal([firstInsurance.Id], await IdsAsync(new DashboardDeadlineFilter(Status: DeadlineStatus.DueSoon)));
        Assert.Equal([firstInspection.Id], await IdsAsync(new DashboardDeadlineFilter(VehicleId: first.Id, Status: DeadlineStatus.Overdue)));
    }

    [Fact]
    public async Task La_ligne_expose_vehicule_type_d_echeance_et_km()
    {
        var ambulance = _vehicleTypes.Seed("Ambulance");
        var vehicle = _vehicles.Seed("AB-123-CD", new VehicleDetails(null, null, null, null, "AMB-1", ambulance.Id));
        var type = _types.Seed("Vidange");
        _readings.Seed(vehicle.Id, Today, 49_500);
        var deadline = AddDeadline(vehicle, type, null, 50_000);

        var line = Assert.Single(await UseCase.ExecuteAsync(new DashboardDeadlineFilter(), CancellationToken.None));

        Assert.Equal(deadline.Id, line.Id);
        Assert.Equal(new DashboardVehicleDto(vehicle.Id, "AB-123-CD", "AMB-1", ambulance.Id, "Ambulance"), line.Vehicle);
        Assert.Equal(new DashboardDeadlineTypeDto(type.Id, "Vidange"), line.DeadlineType);
        Assert.Equal(DeadlineStatus.DueSoon, line.Status);
        Assert.Null(line.DueDate);
        Assert.Equal(50_000, line.DueKm);
        Assert.Equal(49_500, line.CurrentKm);
    }

    [Fact]
    public async Task Le_km_actuel_est_charge_en_une_seule_requete_quel_que_soit_le_nombre_de_vehicules()
    {
        var type = _types.Seed("Contrôle technique");
        for (var i = 0; i < 5; i++)
        {
            AddDeadline(_vehicles.Seed($"AB-12{i}-CD"), type, Today.AddDays(-1));
        }

        Assert.Equal(5, (await IdsAsync()).Count());
        Assert.Equal(1, _readings.CurrentByVehicleCalls);
    }
}
