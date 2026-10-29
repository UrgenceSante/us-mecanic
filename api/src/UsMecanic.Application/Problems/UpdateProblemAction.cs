using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Modifie l'échéance, le commentaire et l'intervention liée d'une action ouverte (l'acteur, le type et la
/// contrainte ne changent pas). Une action clôturée, un véhicule sorti du parc ou une intervention d'un autre
/// véhicule sont refusés.
/// </summary>
public sealed class UpdateProblemAction(
    IProblemRepository problems,
    MileageVehicleGuard guard,
    ProblemActionInterventionCheck interventionCheck)
{
    public async Task<Result<ProblemActionDto>> ExecuteAsync(Guid problemId, Guid actionId, UpdateProblemActionInput input, CancellationToken cancellationToken)
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

        var interventionError = await interventionCheck.CheckAsync(problem.VehicleId, input.InterventionId, cancellationToken);
        if (interventionError is not null)
        {
            return Result<ProblemActionDto>.Failure(interventionError);
        }

        var updated = problem.UpdateAction(actionId, input.DueDate, input.Comment, input.InterventionId);
        if (updated.IsFailure)
        {
            return Result<ProblemActionDto>.Failure(updated.Error!);
        }

        await problems.SaveChangesAsync(cancellationToken);
        return Result<ProblemActionDto>.Success(ProblemActionDto.From(updated.Value));
    }
}
