using UsMecanic.Application.Mileage;
using UsMecanic.Application.Problems;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Problems;

public class ProblemUseCasesTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryProblemRepository _problems = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;

    public ProblemUseCasesTests() => _vehicle = _vehicles.Seed("AB-123-CD");

    private ReportProblem Report => new(_vehicles, new MileageVehicleGuard(_vehicles), _problems, _clock, _clock.Calendar);

    private ListVehicleProblems List => new(new MileageVehicleGuard(_vehicles), _problems);

    private Task<Result<ProblemDto>> ReportAsync(string? plate = "AB-123-CD", DateOnly? date = null, string? description = "Pneu crevé", string? author = "D. Martin") =>
        Report.ExecuteAsync(new ReportProblemInput(plate, date ?? Today, description, author), CancellationToken.None);

    [Fact]
    public async Task Un_signalement_valide_cree_un_probleme_signale_et_l_enregistre()
    {
        var result = await ReportAsync(plate: "ab 123 cd", date: Today.AddDays(-1));

        Assert.True(result.IsSuccess);
        Assert.Equal(_vehicle.Id, result.Value.VehicleId);
        Assert.Equal(ProblemStatus.Reported, result.Value.Status);
        Assert.Equal("Pneu crevé", result.Value.Description);
        Assert.Equal("D. Martin", result.Value.Author);
        Assert.Equal(Today.AddDays(-1), result.Value.Date);
        Assert.Equal(result.Value.Id, Assert.Single(_problems.Saved).Id);
        Assert.Equal(1, _problems.SaveCount);
    }

    [Fact]
    public async Task Une_immatriculation_inconnue_est_refusee_sans_rien_enregistrer()
    {
        var result = await ReportAsync(plate: "ZZ-999-ZZ");

        Assert.Equal(ProblemErrors.VehicleUnknown, result.Error);
        Assert.Empty(_problems.Saved);
        Assert.Equal(0, _problems.SaveCount);
    }

    [Fact]
    public async Task Une_immatriculation_mal_formee_est_refusee()
    {
        var result = await ReportAsync(plate: "n'importe quoi");

        Assert.Equal(LicensePlate.InvalidFormat, result.Error);
        Assert.Empty(_problems.Saved);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_est_refuse()
    {
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var result = await ReportAsync();

        Assert.Equal(VehicleErrors.Retired, result.Error);
        Assert.Empty(_problems.Saved);
    }

    [Fact]
    public async Task Un_auteur_absent_ou_une_description_absente_sont_refuses()
    {
        var noAuthor = await ReportAsync(author: "  ");
        var noDescription = await ReportAsync(description: null);
        var future = await ReportAsync(date: Today.AddDays(1));

        Assert.Equal(ProblemErrors.AuthorRequired, noAuthor.Error);
        Assert.Equal(ProblemErrors.DescriptionRequired, noDescription.Error);
        Assert.Equal(ProblemErrors.DateInFuture, future.Error);
        Assert.Empty(_problems.Saved);
    }

    [Fact]
    public async Task La_liste_est_limitee_au_vehicule_et_triee_du_plus_recent_au_plus_ancien()
    {
        var other = _vehicles.Seed("EF-456-GH");
        var oldest = (await ReportAsync(date: Today.AddDays(-5), description: "Ancien")).Value;
        var newest = (await ReportAsync(date: Today.AddDays(-1), description: "Récent")).Value;
        await Report.ExecuteAsync(new ReportProblemInput("EF-456-GH", Today, "Autre véhicule", "D. Martin"), CancellationToken.None);

        var result = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { newest.Id, oldest.Id }, result.Value.Select(p => p.Id).ToArray());

        var ofOther = await List.ExecuteAsync(other.Id, CancellationToken.None);
        Assert.Equal("Autre véhicule", Assert.Single(ofOther.Value).Description);
    }

    [Fact]
    public async Task La_liste_d_un_vehicule_inconnu_est_refusee_et_celle_d_un_vehicule_sorti_du_parc_reste_lisible()
    {
        await ReportAsync();
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var unknown = await List.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);
        var retired = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, unknown.Error);
        Assert.True(retired.IsSuccess);
        Assert.Single(retired.Value);
    }
}
