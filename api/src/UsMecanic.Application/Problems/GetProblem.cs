using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Détail d'un problème, avec son analyse s'il est analysé.</summary>
public sealed class GetProblem(IProblemRepository problems)
{
    public async Task<Result<ProblemDto>> ExecuteAsync(Guid problemId, CancellationToken cancellationToken)
    {
        var problem = await problems.GetByIdAsync(problemId, cancellationToken);
        return problem is null
            ? Result<ProblemDto>.Failure(ProblemErrors.NotFound)
            : Result<ProblemDto>.Success(ProblemDto.From(problem));
    }
}
