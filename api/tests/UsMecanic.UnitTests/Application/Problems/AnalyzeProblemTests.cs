using UsMecanic.Application.Mileage;
using UsMecanic.Application.Problems;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Problems;

public class AnalyzeProblemTests
{
    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryProblemRepository _problems = new();
    private readonly InMemoryProblemReferenceRepository<ProblemNature> _natures = new();
    private readonly InMemoryProblemReferenceRepository<ProblemElement> _elements = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;
    private readonly Problem _problem;
    private readonly ProblemNature _nature;
    private readonly ProblemElement _element;

    public AnalyzeProblemTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
        _problem = Problem.Create(_vehicle.Id, FixedClock.DefaultToday, "Voyant moteur", "D. Martin", FixedClock.DefaultToday, _clock.Now).Value;
        _problems.Saved.Add(_problem);
        _nature = _natures.Seed("Mécanique");
        _element = _elements.Seed("Moteur");
    }

    private AnalyzeProblem UseCase => new(_problems, _vehicles, new MileageVehicleGuard(_vehicles), _natures, _elements, _clock);

    private Task<Result<ProblemDto>> AnalyzeAsync(Guid? problemId = null, Guid? natureId = null, Guid? elementId = null, string? text = "Injecteur HS", bool immobilized = false) =>
        UseCase.ExecuteAsync(problemId ?? _problem.Id, new AnalyzeProblemInput(natureId ?? _nature.Id, elementId ?? _element.Id, text, immobilized), CancellationToken.None);

    [Fact]
    public async Task Analyser_un_probleme_signale_le_passe_a_analyse_sans_toucher_au_vehicule()
    {
        var saves = _problems.SaveCount;

        var result = await AnalyzeAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(ProblemStatus.Analyzed, result.Value.Status);
        var analysis = result.Value.Analysis!;
        Assert.Equal(_nature.Id, analysis.NatureId);
        Assert.Equal(_element.Id, analysis.ElementId);
        Assert.Equal("Injecteur HS", analysis.Text);
        Assert.False(analysis.Immobilized);
        Assert.Equal(_clock.Now, analysis.AnalyzedAt);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
        Assert.Equal(saves + 1, _problems.SaveCount);
    }

    [Fact]
    public async Task Une_immobilisation_cochee_immobilise_le_vehicule()
    {
        var result = await AnalyzeAsync(immobilized: true);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Analysis!.Immobilized);
        Assert.Equal(VehicleStatus.Immobilized, _vehicle.Status);
    }

    [Fact]
    public async Task Un_probleme_inconnu_est_refuse()
    {
        var result = await AnalyzeAsync(problemId: Guid.NewGuid());

        Assert.Equal(ProblemErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Un_vehicule_sorti_du_parc_refuse_l_analyse()
    {
        _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

        var result = await AnalyzeAsync(immobilized: true);

        Assert.Equal(VehicleErrors.Retired, result.Error);
        Assert.Equal(ProblemStatus.Reported, _problem.Status);
        Assert.Equal(VehicleStatus.Retired, _vehicle.Status);
    }

    [Fact]
    public async Task Une_nature_ou_un_element_inconnu_est_refuse_sans_rien_modifier()
    {
        var nature = await AnalyzeAsync(natureId: Guid.NewGuid(), immobilized: true);
        var element = await AnalyzeAsync(elementId: Guid.NewGuid(), immobilized: true);

        Assert.Equal(ProblemErrors.NatureUnknown, nature.Error);
        Assert.Equal(ProblemErrors.ElementUnknown, element.Error);
        Assert.Equal(ProblemStatus.Reported, _problem.Status);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
    }

    [Fact]
    public async Task Une_nature_ou_un_element_inactif_est_refuse()
    {
        var inactiveNature = _natures.Seed("Ancienne nature", active: false);
        var inactiveElement = _elements.Seed("Ancien élément", active: false);

        var nature = await AnalyzeAsync(natureId: inactiveNature.Id);
        var element = await AnalyzeAsync(elementId: inactiveElement.Id);

        Assert.Equal("problem_nature.inactive", nature.Error!.Code);
        Assert.Equal("problem_element.inactive", element.Error!.Code);
        Assert.Equal(ProblemStatus.Reported, _problem.Status);
    }

    [Fact]
    public async Task Un_texte_absent_est_refuse_sans_immobiliser_le_vehicule()
    {
        var result = await AnalyzeAsync(text: " ", immobilized: true);

        Assert.Equal(ProblemErrors.AnalysisRequired, result.Error);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
    }

    [Fact]
    public async Task Une_seconde_analyse_est_refusee_et_ne_change_pas_le_vehicule()
    {
        await AnalyzeAsync(text: "Première");

        var second = await AnalyzeAsync(text: "Seconde", immobilized: true);

        Assert.Equal(ProblemErrors.AlreadyAnalyzed, second.Error);
        Assert.Equal("Première", _problem.Analysis!.Text);
        Assert.Equal(VehicleStatus.Active, _vehicle.Status);
    }

    [Fact]
    public async Task Le_detail_d_un_probleme_porte_son_analyse()
    {
        var before = await new GetProblem(_problems).ExecuteAsync(_problem.Id, CancellationToken.None);
        await AnalyzeAsync();
        var after = await new GetProblem(_problems).ExecuteAsync(_problem.Id, CancellationToken.None);
        var unknown = await new GetProblem(_problems).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(before.Value.Analysis);
        Assert.NotNull(after.Value.Analysis);
        Assert.Equal(ProblemErrors.NotFound, unknown.Error);
    }
}
