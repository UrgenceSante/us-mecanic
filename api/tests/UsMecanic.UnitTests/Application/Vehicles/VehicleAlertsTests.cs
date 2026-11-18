using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

public class VehicleAlertsTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryDeadlineTypeRepository _types = new();
    private readonly InMemoryVehicleDeadlineRepository _deadlines = new();
    private readonly InMemoryProblemRepository _problems = new();
    private readonly InMemoryMileageReadingRepository _readings = new();

    private async Task<IReadOnlyList<VehicleDto>> ListAsync()
    {
        var alerts = new VehicleAlertsLookup(_deadlines, _types, new InMemoryDeadlineThresholdsRepository(), _problems, new FixedClock().Calendar);
        var lookup = new VehicleTypeLookup(new InMemoryVehicleTypeRepository(), _readings, new VehicleOwnerLookup(new InMemoryCompanyRepository(), new InMemoryRentalRepository(), new FixedClock().Calendar));
        return await new ListVehicles(_vehicles, lookup, alerts).ExecuteAsync(new VehicleFilter(), CancellationToken.None);
    }

    private void AddDeadline(Vehicle vehicle, DateOnly? dueDate, int? dueKm = null)
    {
        var type = _types.Saved.Count > 0 ? _types.Saved[0] : _types.Seed("Vidange");
        var deadline = VehicleDeadline.Create(vehicle.Id, type, new VehicleDeadlineSchedule(null, null, null, null, dueDate, dueKm), Today).Value;
        _deadlines.Saved.Add(deadline);
    }

    private void AddProblem(Vehicle vehicle, bool immobilized, bool resolve = false)
    {
        var problem = Problem.Create(vehicle.Id, Today.AddDays(-1), "Voyant", "D. Martin", Today, Now).Value;
        problem.Analyze(Guid.NewGuid(), Guid.NewGuid(), "Analyse", immobilized, Now);
        if (resolve)
        {
            var action = problem.AddAction(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null, null, Now).Value;
            problem.CloseAction(action.Id, Now);
        }

        _problems.Saved.Add(problem);
    }

    [Fact]
    public async Task Un_vehicule_sans_echeance_ni_probleme_n_a_aucune_alerte()
    {
        _vehicles.Seed("AB-123-CD");

        var alerts = (await ListAsync()).Single().Alerts;

        Assert.Equal(new VehicleAlertsDto(0, false), alerts);
    }

    [Fact]
    public async Task Les_echeances_depassees_par_date_ou_par_km_sont_comptees_par_vehicule()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var other = _vehicles.Seed("EF-456-GH");
        _readings.Seed(vehicle.Id, Today, 50_500);
        AddDeadline(vehicle, Today.AddDays(-3));
        AddDeadline(vehicle, null, 50_000);
        AddDeadline(vehicle, Today.AddDays(200));
        AddDeadline(other, Today.AddDays(200));

        var list = await ListAsync();

        Assert.Equal(2, list.Single(v => v.Id == vehicle.Id).Alerts?.OverdueDeadlines);
        Assert.Equal(0, list.Single(v => v.Id == other.Id).Alerts?.OverdueDeadlines);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_n_a_aucune_alerte()
    {
        var retired = _vehicles.Seed("AB-123-CD");
        AddDeadline(retired, Today.AddDays(-3));
        AddProblem(retired, immobilized: true);
        retired.ChangeStatus(VehicleStatus.Retired, Now);

        var alerts = (await ListAsync()).Single().Alerts;

        Assert.Equal(new VehicleAlertsDto(0, false), alerts);
    }

    [Fact]
    public async Task Seul_un_probleme_immobilisant_non_resolu_declenche_l_alerte()
    {
        var blocked = _vehicles.Seed("AB-123-CD");
        var resolved = _vehicles.Seed("EF-456-GH");
        var minor = _vehicles.Seed("IJ-789-KL");
        AddProblem(blocked, immobilized: true);
        AddProblem(resolved, immobilized: true, resolve: true);
        AddProblem(minor, immobilized: false);

        var list = await ListAsync();

        Assert.True(list.Single(v => v.Id == blocked.Id).Alerts?.ImmobilizingProblem);
        Assert.False(list.Single(v => v.Id == resolved.Id).Alerts?.ImmobilizingProblem);
        Assert.False(list.Single(v => v.Id == minor.Id).Alerts?.ImmobilizingProblem);
    }
}
