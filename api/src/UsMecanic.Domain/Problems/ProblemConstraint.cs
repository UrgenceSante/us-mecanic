using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>Contrainte de délai associée à une action (immédiat, sous 7 jours…).</summary>
public sealed class ProblemConstraint : ProblemReference, IProblemReference<ProblemConstraint>
{
    private ProblemConstraint(Guid id, string name)
        : base(id, name, Definition)
    {
    }

    public static ReferenceKind Definition { get; } = new("problem_constraint", "Contrainte");

    public static Result<ProblemConstraint> Create(string? name) =>
        CreateValidated(name, Definition, cleaned => new ProblemConstraint(Guid.CreateVersion7(), cleaned));
}
