namespace UsMecanic.Domain.Problems;

/// <summary>
/// Action de résolution d'un problème analysé : acteur, type et contrainte (référentiels), échéance et commentaire
/// facultatifs, intervention du carnet d'entretien liée (facultative, même véhicule) et clôture. Elle ne se crée et
/// ne se modifie que par le <see cref="Problem"/> qui la porte.
/// </summary>
public sealed class ProblemAction
{
    public const int MaxCommentLength = 2000;

    // Les paramètres portent le nom des propriétés : c'est ce qui permet à EF Core de rematérialiser l'entité.
    internal ProblemAction(
        Guid id,
        Guid problemId,
        Guid actorId,
        Guid typeId,
        Guid constraintId,
        DateOnly? dueDate,
        string? comment,
        Guid? interventionId,
        DateTimeOffset recordedAt,
        DateTimeOffset? closedAt)
    {
        Id = id;
        ProblemId = problemId;
        ActorId = actorId;
        TypeId = typeId;
        ConstraintId = constraintId;
        DueDate = dueDate;
        Comment = comment;
        InterventionId = interventionId;
        RecordedAt = recordedAt;
        ClosedAt = closedAt;
    }

    public Guid Id { get; }

    public Guid ProblemId { get; }

    public Guid ActorId { get; }

    public Guid TypeId { get; }

    public Guid ConstraintId { get; }

    public DateOnly? DueDate { get; private set; }

    public string? Comment { get; private set; }

    /// <summary>Intervention du même véhicule liée à l'action ; retirée si l'intervention est supprimée.</summary>
    public Guid? InterventionId { get; private set; }

    public DateTimeOffset RecordedAt { get; }

    /// <summary>Date de clôture, ou <c>null</c> tant que l'action est ouverte.</summary>
    public DateTimeOffset? ClosedAt { get; private set; }

    public bool IsClosed => ClosedAt is not null;

    internal void Update(DateOnly? dueDate, string? comment, Guid? interventionId)
    {
        DueDate = dueDate;
        Comment = comment;
        InterventionId = interventionId;
    }

    internal void Close(DateTimeOffset now) => ClosedAt = now;
}
