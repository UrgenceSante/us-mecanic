using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

/// <summary>
/// Réalisation d'une échéance : date réelle, km facultatif, commentaire facultatif et date de saisie.
/// La réalisation est conservée même quand l'échéance périodique est ensuite recalculée.
/// </summary>
public sealed class DeadlineCompletion
{
    public const int MaxCommentLength = 1000;

    private DeadlineCompletion(Guid id, Guid vehicleDeadlineId, DateOnly date, int? kilometers, string? comment, DateTimeOffset recordedAt)
    {
        Id = id;
        VehicleDeadlineId = vehicleDeadlineId;
        Date = date;
        Kilometers = kilometers;
        Comment = comment;
        RecordedAt = recordedAt;
    }

    public Guid Id { get; }

    public Guid VehicleDeadlineId { get; }

    /// <summary>Date réelle de réalisation (et non de saisie).</summary>
    public DateOnly Date { get; }

    public int? Kilometers { get; }

    public string? Comment { get; }

    /// <summary>Date de saisie de la réalisation.</summary>
    public DateTimeOffset RecordedAt { get; }

    /// <summary>Crée la réalisation ; la validité de la date et du km est contrôlée par <see cref="VehicleDeadline.Complete"/>.</summary>
    public static Result<DeadlineCompletion> Create(Guid vehicleDeadlineId, DateOnly date, int? kilometers, string? comment, DateTimeOffset now)
    {
        var cleaned = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        return cleaned?.Length > MaxCommentLength
            ? Result<DeadlineCompletion>.Failure(DeadlineCompletionErrors.CommentTooLong)
            : Result<DeadlineCompletion>.Success(new DeadlineCompletion(Guid.CreateVersion7(now), vehicleDeadlineId, date, kilometers, cleaned, now));
    }
}
