using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>
/// Problème signalé sur un véhicule : date du signalement, description, auteur (identifiant texte envoyé par
/// l'application chauffeur, donnée personnelle), statut et date de saisie. Il est créé au statut « Signalé ».
/// </summary>
public sealed class Problem
{
    public const int MaxDescriptionLength = 2000;

    public const int MaxAuthorLength = 200;

    public const int MaxAnalysisLength = 4000;

    // Les paramètres portent le nom des propriétés : c'est ce qui permet à EF Core de rematérialiser l'entité.
    private Problem(Guid id, Guid vehicleId, DateOnly date, string description, string author, ProblemStatus status, DateTimeOffset recordedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        Date = date;
        Description = description;
        Author = author;
        Status = status;
        RecordedAt = recordedAt;
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    /// <summary>Date du signalement (et non de sa saisie).</summary>
    public DateOnly Date { get; }

    public string Description { get; }

    /// <summary>Auteur du signalement : donnée personnelle, jamais écrite dans les logs.</summary>
    public string Author { get; }

    public ProblemStatus Status { get; private set; }

    // Analyse à plat sur la table des problèmes (1-1, colonnes nullables tant que non analysé) : les actions de
    // résolution (0..n) se rattacheront au problème. Ces propriétés sont persistées ; la lecture passe par Analysis.
    public Guid? NatureId { get; private set; }

    public Guid? ElementId { get; private set; }

    public string? AnalysisText { get; private set; }

    public bool? Immobilized { get; private set; }

    public DateTimeOffset? AnalyzedAt { get; private set; }

    /// <summary>Analyse du problème, ou <c>null</c> s'il est encore au statut « Signalé ».</summary>
    public ProblemAnalysis? Analysis =>
        NatureId is { } natureId && ElementId is { } elementId && AnalysisText is { } text && AnalyzedAt is { } analyzedAt
            ? new ProblemAnalysis(natureId, elementId, text, Immobilized == true, analyzedAt)
            : null;

    /// <summary>Date de saisie du signalement.</summary>
    public DateTimeOffset RecordedAt { get; }

    /// <summary>
    /// Crée un problème au statut « Signalé » : description et auteur obligatoires et bornés, date pas dans le futur.
    /// L'existence du véhicule et sa présence dans le parc sont contrôlées par le cas d'usage.
    /// </summary>
    public static Result<Problem> Create(Guid vehicleId, DateOnly date, string? description, string? author, DateOnly today, DateTimeOffset now)
    {
        var cleanedDescription = description?.Trim();
        if (string.IsNullOrEmpty(cleanedDescription))
        {
            return Result<Problem>.Failure(ProblemErrors.DescriptionRequired);
        }

        if (cleanedDescription.Length > MaxDescriptionLength)
        {
            return Result<Problem>.Failure(ProblemErrors.DescriptionTooLong);
        }

        var cleanedAuthor = author?.Trim();
        if (string.IsNullOrEmpty(cleanedAuthor))
        {
            return Result<Problem>.Failure(ProblemErrors.AuthorRequired);
        }

        if (cleanedAuthor.Length > MaxAuthorLength)
        {
            return Result<Problem>.Failure(ProblemErrors.AuthorTooLong);
        }

        if (date > today)
        {
            return Result<Problem>.Failure(ProblemErrors.DateInFuture);
        }

        var problem = new Problem(Guid.CreateVersion7(now), vehicleId, date, cleanedDescription, cleanedAuthor, ProblemStatus.Reported, now);
        return Result<Problem>.Success(problem);
    }

    /// <summary>
    /// Analyse le problème : « Signalé » devient « Analysé ». Un problème déjà analysé ou résolu est refusé (la
    /// modification d'une analyse n'est pas encore spécifiée). Référentiels, statut du véhicule et immobilisation
    /// sont gérés par le cas d'usage.
    /// </summary>
    public Result<Problem> Analyze(Guid natureId, Guid elementId, string? text, bool immobilized, DateTimeOffset now)
    {
        if (Status == ProblemStatus.Resolved)
        {
            return Result<Problem>.Failure(ProblemErrors.AlreadyResolved);
        }

        if (Status == ProblemStatus.Analyzed)
        {
            return Result<Problem>.Failure(ProblemErrors.AlreadyAnalyzed);
        }

        var cleaned = text?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<Problem>.Failure(ProblemErrors.AnalysisRequired);
        }

        if (cleaned.Length > MaxAnalysisLength)
        {
            return Result<Problem>.Failure(ProblemErrors.AnalysisTooLong);
        }

        NatureId = natureId;
        ElementId = elementId;
        AnalysisText = cleaned;
        Immobilized = immobilized;
        AnalyzedAt = now;
        Status = ProblemStatus.Analyzed;
        return Result<Problem>.Success(this);
    }
}
