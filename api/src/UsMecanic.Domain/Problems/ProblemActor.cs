using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>Acteur intervenant sur un problème (garage, atelier interne…).</summary>
public sealed class ProblemActor : ProblemReference, IProblemReference<ProblemActor>
{
    private ProblemActor(Guid id, string name)
        : base(id, name, Definition)
    {
    }

    public static ReferenceKind Definition { get; } = new("problem_actor", "Acteur");

    public static Result<ProblemActor> Create(string? name) =>
        CreateValidated(name, Definition, cleaned => new ProblemActor(Guid.CreateVersion7(), cleaned));
}
