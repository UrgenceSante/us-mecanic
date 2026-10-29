using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems.References;

/// <summary>Liste un référentiel des problèmes. Les valeurs désactivées ne sont plus proposées, sauf demande explicite.</summary>
public sealed class ListProblemReferences<T>(IProblemReferenceRepository<T> repository)
    where T : ProblemReference, IProblemReference<T>
{
    public async Task<IReadOnlyList<ProblemReferenceDto>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var items = await repository.ListAsync(includeInactive, cancellationToken);
        return [.. items.Select(ProblemReferenceDto.From)];
    }
}
