using UsMecanic.Application.Mileage;
using UsMecanic.Application.Problems.References;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Ajoute une action de résolution à un problème analysé. Acteur, type et contrainte doivent exister et être actifs,
/// l'intervention éventuelle doit appartenir au même véhicule. Un problème non analysé ou un véhicule sorti du parc
/// sont refusés. Les changements de statut du problème et du véhicule ne sont pas gérés ici.
/// </summary>
public sealed class AddProblemAction(
    IProblemRepository problems,
    MileageVehicleGuard guard,
    IProblemReferenceRepository<ProblemActor> actors,
    IProblemReferenceRepository<ProblemActionType> types,
    IProblemReferenceRepository<ProblemConstraint> constraints,
    ProblemActionInterventionCheck interventionCheck,
    TimeProvider clock)
{
    public async Task<Result<ProblemActionDto>> ExecuteAsync(Guid problemId, AddProblemActionInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var problem = await problems.GetByIdAsync(problemId, cancellationToken);
        if (problem is null)
        {
            return Result<ProblemActionDto>.Failure(ProblemErrors.NotFound);
        }

        var guardError = await guard.CheckAsync(problem.VehicleId, forWrite: true, cancellationToken);
        if (guardError is not null)
        {
            return Result<ProblemActionDto>.Failure(guardError);
        }

        var actor = await actors.GetByIdAsync(input.ActorId, cancellationToken);
        var actorError = actor is null ? ProblemErrors.ActorUnknown : actor.GetSelectionError();
        if (actorError is not null)
        {
            return Result<ProblemActionDto>.Failure(actorError);
        }

        var type = await types.GetByIdAsync(input.TypeId, cancellationToken);
        var typeError = type is null ? ProblemErrors.ActionTypeUnknown : type.GetSelectionError();
        if (typeError is not null)
        {
            return Result<ProblemActionDto>.Failure(typeError);
        }

        var constraint = await constraints.GetByIdAsync(input.ConstraintId, cancellationToken);
        var constraintError = constraint is null ? ProblemErrors.ConstraintUnknown : constraint.GetSelectionError();
        if (constraintError is not null)
        {
            return Result<ProblemActionDto>.Failure(constraintError);
        }

        var interventionError = await interventionCheck.CheckAsync(problem.VehicleId, input.InterventionId, cancellationToken);
        if (interventionError is not null)
        {
            return Result<ProblemActionDto>.Failure(interventionError);
        }

        var added = problem.AddAction(input.ActorId, input.TypeId, input.ConstraintId, input.DueDate, input.Comment, input.InterventionId, clock.GetUtcNow());
        if (added.IsFailure)
        {
            return Result<ProblemActionDto>.Failure(added.Error!);
        }

        await problems.SaveChangesAsync(cancellationToken);
        return Result<ProblemActionDto>.Success(ProblemActionDto.From(added.Value));
    }
}
