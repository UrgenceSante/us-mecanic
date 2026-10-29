using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>Nature d'un problème (mécanique, carrosserie…).</summary>
public sealed class ProblemNature : ProblemReference, IProblemReference<ProblemNature>
{
    private ProblemNature(Guid id, string name)
        : base(id, name, Definition)
    {
    }

    public static ReferenceKind Definition { get; } = new("problem_nature", "Nature");

    public static Result<ProblemNature> Create(string? name) =>
        CreateValidated(name, Definition, cleaned => new ProblemNature(Guid.CreateVersion7(), cleaned));
}
