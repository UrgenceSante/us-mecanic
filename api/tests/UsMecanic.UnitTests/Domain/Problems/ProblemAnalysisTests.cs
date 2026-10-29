using UsMecanic.Domain.Problems;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Problems;

public class ProblemAnalysisTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;
    private readonly Guid _natureId = Guid.NewGuid();
    private readonly Guid _elementId = Guid.NewGuid();

    private static Problem NewProblem() =>
        Problem.Create(Guid.NewGuid(), FixedClock.DefaultToday, "Voyant moteur", "D. Martin", FixedClock.DefaultToday, Now).Value;

    [Fact]
    public void Un_probleme_signale_n_a_pas_d_analyse()
    {
        var problem = NewProblem();

        Assert.Null(problem.Analysis);
        Assert.Equal(ProblemStatus.Reported, problem.Status);
    }

    [Fact]
    public void Analyze_passe_le_probleme_a_analyse_et_enregistre_l_analyse_nettoyee()
    {
        var problem = NewProblem();
        var later = Now.AddHours(2);

        var result = problem.Analyze(_natureId, _elementId, "  Injecteur défaillant ", true, later);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProblemStatus.Analyzed, problem.Status);
        Assert.Equal(new ProblemAnalysis(_natureId, _elementId, "Injecteur défaillant", true, later), problem.Analysis);
    }

    [Fact]
    public void Analyze_sans_immobilisation_la_conserve_a_non()
    {
        var problem = NewProblem();

        problem.Analyze(_natureId, _elementId, "Usure normale", false, Now);

        Assert.False(problem.Analysis!.Immobilized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Analyze_refuse_un_texte_absent_sans_modifier_le_probleme(string? text)
    {
        var problem = NewProblem();

        var result = problem.Analyze(_natureId, _elementId, text, false, Now);

        Assert.Equal(ProblemErrors.AnalysisRequired, result.Error);
        Assert.Equal(ProblemStatus.Reported, problem.Status);
        Assert.Null(problem.Analysis);
    }

    [Fact]
    public void Analyze_borne_le_texte()
    {
        Assert.True(NewProblem().Analyze(_natureId, _elementId, new string('a', Problem.MaxAnalysisLength), false, Now).IsSuccess);
        Assert.Equal(
            ProblemErrors.AnalysisTooLong,
            NewProblem().Analyze(_natureId, _elementId, new string('a', Problem.MaxAnalysisLength + 1), false, Now).Error);
    }

    [Fact]
    public void Analyze_refuse_un_probleme_deja_analyse_et_conserve_la_premiere_analyse()
    {
        var problem = NewProblem();
        problem.Analyze(_natureId, _elementId, "Première", true, Now);

        var result = problem.Analyze(Guid.NewGuid(), Guid.NewGuid(), "Seconde", false, Now.AddDays(1));

        Assert.Equal(ProblemErrors.AlreadyAnalyzed, result.Error);
        Assert.Equal(new ProblemAnalysis(_natureId, _elementId, "Première", true, Now), problem.Analysis);
    }
}
