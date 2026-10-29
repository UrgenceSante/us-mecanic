using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Clôture une action de résolution. Une action déjà clôturée ou un véhicule sorti du parc sont refusés. Le passage
/// automatique du problème à « Résolu » et le retour en service du véhicule ne sont pas gérés ici.
/// </summary>
public sealed class CloseProblemAction(IProblemRepository problems, MileageVehicleGuard guard, TimeProvider clock)
{
    public async Task<Result<ProblemActionDto>> ExecuteAsync(Guid problemId, Guid actionId, CancellationToken cancellationToken)
    {
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

        var closed = problem.CloseAction(actionId, clock.GetUtcNow());
        if (closed.IsFailure)
        {
            return Result<ProblemActionDto>.Failure(closed.Error!);
        }

        await problems.SaveChangesAsync(cancellationToken);
        return Result<ProblemActionDto>.Success(ProblemActionDto.From(closed.Value));
    }
}
