using UsMecanic.Domain.Problems;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Domain.Problems;

public class ProblemActionTests
{
    private static readonly DateTimeOffset Now = FixedClock.Default;
    private readonly Guid _actorId = Guid.NewGuid();
    private readonly Guid _typeId = Guid.NewGuid();
    private readonly Guid _constraintId = Guid.NewGuid();

    private static Problem NewProblem(bool analyzed = true)
    {
        var problem = Problem.Create(Guid.NewGuid(), FixedClock.DefaultToday, "Voyant moteur", "D. Martin", FixedClock.DefaultToday, Now).Value;
        if (analyzed)
        {
            problem.Analyze(Guid.NewGuid(), Guid.NewGuid(), "Injecteur HS", false, Now);
        }

        return problem;
    }

    [Fact]
    public void AddAction_rattache_une_action_ouverte_au_probleme_analyse()
    {
        var problem = NewProblem();
        var due = FixedClock.DefaultToday.AddDays(10);
        var interventionId = Guid.NewGuid();

        var result = problem.AddAction(_actorId, _typeId, _constraintId, due, "  Prendre rendez-vous ", interventionId, Now);

        Assert.True(result.IsSuccess);
        var action = Assert.Single(problem.Actions);
        Assert.Same(action, result.Value);
        Assert.Equal(problem.Id, action.ProblemId);
        Assert.Equal(_actorId, action.ActorId);
        Assert.Equal(_typeId, action.TypeId);
        Assert.Equal(_constraintId, action.ConstraintId);
        Assert.Equal(due, action.DueDate);
        Assert.Equal("Prendre rendez-vous", action.Comment);
        Assert.Equal(interventionId, action.InterventionId);
        Assert.Equal(Now, action.RecordedAt);
        Assert.False(action.IsClosed);
        Assert.Null(action.ClosedAt);
        Assert.Equal(ProblemStatus.Analyzed, problem.Status);
    }

    [Fact]
    public void AddAction_accepte_zero_a_n_actions_et_les_champs_facultatifs_absents()
    {
        var problem = NewProblem();
        Assert.Empty(problem.Actions);

        problem.AddAction(_actorId, _typeId, _constraintId, null, " ", null, Now);
        problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now);

        Assert.Equal(2, problem.Actions.Count);
        Assert.All(problem.Actions, a =>
        {
            Assert.Null(a.DueDate);
            Assert.Null(a.Comment);
            Assert.Null(a.InterventionId);
        });
    }

    [Fact]
    public void AddAction_est_refusee_sur_un_probleme_non_analyse()
    {
        var problem = NewProblem(analyzed: false);

        var result = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now);

        Assert.Equal(ProblemErrors.NotAnalyzed, result.Error);
        Assert.Empty(problem.Actions);
    }

    [Fact]
    public void AddAction_refuse_un_commentaire_trop_long_mais_accepte_la_limite()
    {
        var problem = NewProblem();

        var tooLong = problem.AddAction(_actorId, _typeId, _constraintId, null, new string('x', ProblemAction.MaxCommentLength + 1), null, Now);
        var limit = problem.AddAction(_actorId, _typeId, _constraintId, null, new string('x', ProblemAction.MaxCommentLength), null, Now);

        Assert.Equal(ProblemErrors.ActionCommentTooLong, tooLong.Error);
        Assert.True(limit.IsSuccess);
        Assert.Single(problem.Actions);
    }

    [Fact]
    public void UpdateAction_remplace_echeance_commentaire_et_intervention()
    {
        var problem = NewProblem();
        var action = problem.AddAction(_actorId, _typeId, _constraintId, FixedClock.DefaultToday, "Avant", null, Now).Value;
        var due = FixedClock.DefaultToday.AddDays(3);
        var interventionId = Guid.NewGuid();

        var result = problem.UpdateAction(action.Id, due, " Après ", interventionId);

        Assert.True(result.IsSuccess);
        Assert.Equal(due, action.DueDate);
        Assert.Equal("Après", action.Comment);
        Assert.Equal(interventionId, action.InterventionId);
        Assert.Equal(_actorId, action.ActorId);

        problem.UpdateAction(action.Id, null, null, null);
        Assert.Null(action.DueDate);
        Assert.Null(action.Comment);
        Assert.Null(action.InterventionId);
    }

    [Fact]
    public void UpdateAction_refuse_une_action_inconnue_cloturee_ou_un_commentaire_trop_long()
    {
        var problem = NewProblem();
        var action = problem.AddAction(_actorId, _typeId, _constraintId, null, "Avant", null, Now).Value;

        var unknown = problem.UpdateAction(Guid.NewGuid(), null, null, null);
        var tooLong = problem.UpdateAction(action.Id, null, new string('x', ProblemAction.MaxCommentLength + 1), null);
        problem.CloseAction(action.Id, Now);
        var closed = problem.UpdateAction(action.Id, null, "Après", null);

        Assert.Equal(ProblemErrors.ActionNotFound, unknown.Error);
        Assert.Equal(ProblemErrors.ActionCommentTooLong, tooLong.Error);
        Assert.Equal(ProblemErrors.ActionAlreadyClosed, closed.Error);
        Assert.Equal("Avant", action.Comment);
    }

    [Fact]
    public void CloseAction_cloture_l_action_a_la_date_donnee()
    {
        var problem = NewProblem();
        var action = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now).Value;
        var later = Now.AddDays(2);

        var result = problem.CloseAction(action.Id, later);

        Assert.True(result.IsSuccess);
        Assert.True(action.IsClosed);
        Assert.Equal(later, action.ClosedAt);
    }

    [Fact]
    public void CloseAction_refuse_une_action_inconnue_ou_deja_cloturee()
    {
        var problem = NewProblem();
        var action = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now).Value;
        problem.CloseAction(action.Id, Now);

        var again = problem.CloseAction(action.Id, Now.AddDays(1));
        var unknown = problem.CloseAction(Guid.NewGuid(), Now);

        Assert.Equal(ProblemErrors.ActionAlreadyClosed, again.Error);
        Assert.Equal(Now, action.ClosedAt);
        Assert.Equal(ProblemErrors.ActionNotFound, unknown.Error);
    }

    [Fact]
    public void CloseAction_resout_le_probleme_quand_toutes_les_actions_sont_closes()
    {
        var problem = NewProblem();
        var first = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now).Value;
        var second = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now).Value;

        problem.CloseAction(first.Id, Now);
        Assert.Equal(ProblemStatus.Analyzed, problem.Status);

        problem.CloseAction(second.Id, Now);
        Assert.Equal(ProblemStatus.Resolved, problem.Status);
    }

    [Fact]
    public void Un_probleme_analyse_sans_action_reste_analyse()
    {
        var problem = NewProblem();

        Assert.Empty(problem.Actions);
        Assert.Equal(ProblemStatus.Analyzed, problem.Status);
    }

    [Fact]
    public void AddAction_rouvre_un_probleme_resolu()
    {
        var problem = NewProblem();
        var action = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now).Value;
        problem.CloseAction(action.Id, Now);
        Assert.Equal(ProblemStatus.Resolved, problem.Status);

        var result = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProblemStatus.Analyzed, problem.Status);
        Assert.Equal(2, problem.Actions.Count);
    }

    [Fact]
    public void AddAction_refusee_ne_rouvre_pas_un_probleme_resolu()
    {
        var problem = NewProblem();
        var action = problem.AddAction(_actorId, _typeId, _constraintId, null, null, null, Now).Value;
        problem.CloseAction(action.Id, Now);

        var result = problem.AddAction(_actorId, _typeId, _constraintId, null, new string('x', ProblemAction.MaxCommentLength + 1), null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(ProblemStatus.Resolved, problem.Status);
    }
}
