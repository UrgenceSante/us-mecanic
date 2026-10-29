using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Action de résolution : référentiels (acteur, type, contrainte), échéance, commentaire, intervention liée, clôture.</summary>
public sealed record ProblemActionDto(
    Guid Id,
    Guid ProblemId,
    Guid ActorId,
    Guid TypeId,
    Guid ConstraintId,
    DateOnly? DueDate,
    string? Comment,
    Guid? InterventionId,
    bool IsClosed,
    DateTimeOffset? ClosedAt,
    DateTimeOffset RecordedAt)
{
    public static ProblemActionDto From(ProblemAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return new ProblemActionDto(
            action.Id,
            action.ProblemId,
            action.ActorId,
            action.TypeId,
            action.ConstraintId,
            action.DueDate,
            action.Comment,
            action.InterventionId,
            action.IsClosed,
            action.ClosedAt,
            action.RecordedAt);
    }
}
