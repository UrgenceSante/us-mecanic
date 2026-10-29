using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems.References;

/// <summary>Renomme une valeur d'un référentiel. Le nouveau libellé doit rester unique dans le référentiel.</summary>
public sealed class RenameProblemReference<T>(IProblemReferenceRepository<T> repository)
    where T : ProblemReference, IProblemReference<T>
{
    public async Task<Result<ProblemReferenceDto>> ExecuteAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        var normalized = ProblemReference.NormalizeName(name, T.Definition);
        if (normalized.IsFailure)
        {
            return Result<ProblemReferenceDto>.Failure(normalized.Error!);
        }

        var item = await repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return Result<ProblemReferenceDto>.Failure(ProblemReferenceErrors.NotFound(T.Definition));
        }

        if (await repository.NameExistsAsync(normalized.Value, id, cancellationToken))
        {
            return Result<ProblemReferenceDto>.Failure(ProblemReferenceErrors.NameAlreadyExists(T.Definition));
        }

        item.Rename(normalized.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<ProblemReferenceDto>.Success(ProblemReferenceDto.From(item));
    }
}
