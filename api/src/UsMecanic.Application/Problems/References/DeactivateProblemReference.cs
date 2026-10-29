using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems.References;

/// <summary>
/// Désactive une valeur d'un référentiel : elle n'est plus acceptée pour une nouvelle saisie
/// mais reste affichée là où elle est déjà utilisée. Il n'existe volontairement aucune suppression.
/// </summary>
public sealed class DeactivateProblemReference<T>(IProblemReferenceRepository<T> repository)
    where T : ProblemReference, IProblemReference<T>
{
    public async Task<Result<ProblemReferenceDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return Result<ProblemReferenceDto>.Failure(ProblemReferenceErrors.NotFound(T.Definition));
        }

        item.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return Result<ProblemReferenceDto>.Success(ProblemReferenceDto.From(item));
    }
}
