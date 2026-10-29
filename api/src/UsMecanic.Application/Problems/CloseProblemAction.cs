using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Clôture une action de résolution. Une action déjà clôturée ou un véhicule sorti du parc sont refusés. Clôturer la
/// dernière action ouverte passe le problème à « Résolu » et peut remettre le véhicule en service, dans la même
/// sauvegarde.
/// </summary>
public sealed class CloseProblemAction(IProblemRepository problems, MileageVehicleGuard guard, ProblemVehicleStatusSync vehicleStatus, TimeProvider clock)
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

        var now = clock.GetUtcNow();
        var previous = problem.Status;
        var closed = problem.CloseAction(actionId, now);
        if (closed.IsFailure)
        {
            return Result<ProblemActionDto>.Failure(closed.Error!);
        }

        var statusError = await vehicleStatus.ApplyAsync(problem, previous, now, cancellationToken);
        if (statusError is not null)
        {
            return Result<ProblemActionDto>.Failure(statusError);
        }

        await problems.SaveChangesAsync(cancellationToken);
        return Result<ProblemActionDto>.Success(ProblemActionDto.From(closed.Value));
    }
}
