using UsMecanic.Domain.Problems;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Problems;

public class ProblemBlockingLevelTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private static Problem NewProblem(bool analyzed, bool immobilized = false)
    {
        var problem = Problem.Create(Guid.NewGuid(), Today, "Voyant moteur", "D. Martin", Today, Now).Value;
        if (analyzed)
        {
            problem.Analyze(Guid.NewGuid(), Guid.NewGuid(), "Injecteur HS", immobilized, Now);
        }

        return problem;
    }

    private static ProblemAction AddAction(Problem problem, DateOnly? due) =>
        problem.AddAction(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), due, null, null, Now).Value;

    [Fact]
    public void Un_probleme_immobilisant_non_resolu_est_immobilisant_meme_avec_une_action_en_retard()
    {
        var problem = NewProblem(analyzed: true, immobilized: true);
        AddAction(problem, Today.AddDays(-3));

        Assert.Equal(ProblemBlockingLevel.Immobilizing, problem.BlockingLevelAt(Today));
    }

    [Fact]
    public void Un_probleme_signale_non_analyse_est_non_analyse()
    {
        Assert.Equal(ProblemBlockingLevel.Unanalyzed, NewProblem(analyzed: false).BlockingLevelAt(Today));
    }

    [Fact]
    public void Un_probleme_analyse_avec_une_action_ouverte_a_echeance_passee_est_en_retard()
    {
        var problem = NewProblem(analyzed: true);
        AddAction(problem, Today.AddDays(5));
        AddAction(problem, Today.AddDays(-1));

        Assert.Equal(ProblemBlockingLevel.OverdueAction, problem.BlockingLevelAt(Today));
    }

    [Fact]
    public void Une_action_close_a_echeance_passee_est_ignoree()
    {
        var problem = NewProblem(analyzed: true);
        var late = AddAction(problem, Today.AddDays(-4));
        AddAction(problem, null);
        problem.CloseAction(late.Id, Now);

        Assert.Equal(ProblemBlockingLevel.Other, problem.BlockingLevelAt(Today));
    }

    [Fact]
    public void Une_echeance_aujourd_hui_ou_future_ou_absente_n_est_pas_en_retard()
    {
        var problem = NewProblem(analyzed: true);
        AddAction(problem, Today);
        AddAction(problem, Today.AddDays(1));
        AddAction(problem, null);

        Assert.Equal(ProblemBlockingLevel.Other, problem.BlockingLevelAt(Today));
        Assert.Equal(ProblemBlockingLevel.OverdueAction, problem.BlockingLevelAt(Today.AddDays(1)));
    }

    [Fact]
    public void Un_probleme_analyse_sans_action_est_autre()
    {
        Assert.Equal(ProblemBlockingLevel.Other, NewProblem(analyzed: true).BlockingLevelAt(Today));
    }

    [Fact]
    public void Un_probleme_immobilisant_resolu_n_est_plus_immobilisant()
    {
        var problem = NewProblem(analyzed: true, immobilized: true);
        var action = AddAction(problem, Today.AddDays(-2));
        problem.CloseAction(action.Id, Now);

        Assert.Equal(ProblemStatus.Resolved, problem.Status);
        Assert.Equal(ProblemBlockingLevel.Other, problem.BlockingLevelAt(Today));
    }
}
