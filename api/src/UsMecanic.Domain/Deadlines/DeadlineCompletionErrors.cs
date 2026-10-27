using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

public static class DeadlineCompletionErrors
{
    public static readonly DomainError DateInFuture =
        new("deadline_completion.date_in_future", "La date de réalisation ne peut pas être dans le futur.");

    public static readonly DomainError KilometersRequired =
        new("deadline_completion.kilometers_required", "Le kilométrage est obligatoire pour réaliser une échéance au km.");

    public static readonly DomainError CommentTooLong =
        new("deadline_completion.comment_too_long", $"Le commentaire est limité à {DeadlineCompletion.MaxCommentLength} caractères.");
}
