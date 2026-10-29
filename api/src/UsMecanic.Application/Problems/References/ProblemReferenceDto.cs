using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems.References;

public sealed record ProblemReferenceDto(Guid Id, string Name, bool IsActive)
{
    public static ProblemReferenceDto From(ProblemReference item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new(item.Id, item.Name, item.IsActive);
    }
}
