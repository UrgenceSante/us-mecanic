using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems.References;

/// <summary>Ajoute une valeur à un référentiel. Le libellé est obligatoire et unique dans le référentiel (sans tenir compte de la casse).</summary>
public sealed class CreateProblemReference<T>(IProblemReferenceRepository<T> repository)
    where T : ProblemReference, IProblemReference<T>
{
    public async Task<Result<ProblemReferenceDto>> ExecuteAsync(string? name, CancellationToken cancellationToken)
    {
        var item = T.Create(name);
        if (item.IsFailure)
        {
            return Result<ProblemReferenceDto>.Failure(item.Error!);
        }

        if (await repository.NameExistsAsync(item.Value.Name, null, cancellationToken))
        {
            return Result<ProblemReferenceDto>.Failure(ProblemReferenceErrors.NameAlreadyExists(T.Definition));
        }

        repository.Add(item.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<ProblemReferenceDto>.Success(ProblemReferenceDto.From(item.Value));
    }
}
