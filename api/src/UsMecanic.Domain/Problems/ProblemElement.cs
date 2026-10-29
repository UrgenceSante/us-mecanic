using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>Élément du véhicule concerné par un problème (moteur, freins…).</summary>
public sealed class ProblemElement : ProblemReference, IProblemReference<ProblemElement>
{
    private ProblemElement(Guid id, string name)
        : base(id, name, Definition)
    {
    }

    public static ReferenceKind Definition { get; } = new("problem_element", "Élément concerné");

    public static Result<ProblemElement> Create(string? name) =>
        CreateValidated(name, Definition, cleaned => new ProblemElement(Guid.CreateVersion7(), cleaned));
}
