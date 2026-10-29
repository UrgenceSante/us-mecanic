using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>Type d'action menée sur un problème (réparation, remplacement…).</summary>
public sealed class ProblemActionType : ProblemReference, IProblemReference<ProblemActionType>
{
    private ProblemActionType(Guid id, string name)
        : base(id, name, Definition)
    {
    }

    public static ReferenceKind Definition { get; } = new("problem_action_type", "Type d'action");

    public static Result<ProblemActionType> Create(string? name) =>
        CreateValidated(name, Definition, cleaned => new ProblemActionType(Guid.CreateVersion7(), cleaned));
}
