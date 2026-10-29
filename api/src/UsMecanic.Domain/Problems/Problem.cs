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

    private readonly List<ProblemAction> _actions = [];

    /// <summary>Actions de résolution (0..n), de la plus ancienne à la plus récente ; vide tant que non analysé.</summary>
    public IReadOnlyList<ProblemAction> Actions => _actions;

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

    /// <summary>
    /// Ajoute une action de résolution à un problème analysé (« Signalé » est refusé). Les référentiels actifs et le
    /// rattachement de l'intervention au véhicule sont contrôlés par le cas d'usage. Sur un problème « Résolu », l'ajout
    /// le rouvre : il repasse à « Analysé » (le statut du véhicule est géré par le cas d'usage).
    /// </summary>
    public Result<ProblemAction> AddAction(Guid actorId, Guid typeId, Guid constraintId, DateOnly? dueDate, string? comment, Guid? interventionId, DateTimeOffset now)
    {
        if (Status == ProblemStatus.Reported)
        {
            return Result<ProblemAction>.Failure(ProblemErrors.NotAnalyzed);
        }

        var cleaned = CleanComment(comment);
        if (cleaned?.Length > ProblemAction.MaxCommentLength)
        {
            return Result<ProblemAction>.Failure(ProblemErrors.ActionCommentTooLong);
        }

        var action = new ProblemAction(Guid.CreateVersion7(now), Id, actorId, typeId, constraintId, dueDate, cleaned, interventionId, now, null);
        _actions.Add(action);
        if (Status == ProblemStatus.Resolved)
        {
            Status = ProblemStatus.Analyzed;
        }

        return Result<ProblemAction>.Success(action);
    }

    /// <summary>Modifie l'échéance, le commentaire et l'intervention liée d'une action ouverte.</summary>
    public Result<ProblemAction> UpdateAction(Guid actionId, DateOnly? dueDate, string? comment, Guid? interventionId)
    {
        var action = _actions.Find(a => a.Id == actionId);
        if (action is null)
        {
            return Result<ProblemAction>.Failure(ProblemErrors.ActionNotFound);
        }

        if (action.IsClosed)
        {
            return Result<ProblemAction>.Failure(ProblemErrors.ActionAlreadyClosed);
        }

        var cleaned = CleanComment(comment);
        if (cleaned?.Length > ProblemAction.MaxCommentLength)
        {
            return Result<ProblemAction>.Failure(ProblemErrors.ActionCommentTooLong);
        }

        action.Update(dueDate, cleaned, interventionId);
        return Result<ProblemAction>.Success(action);
    }

    /// <summary>
    /// Clôture une action ouverte. Quand toutes les actions du problème analysé sont closes, il passe à « Résolu »
    /// (le statut du véhicule est géré par le cas d'usage).
    /// </summary>
    public Result<ProblemAction> CloseAction(Guid actionId, DateTimeOffset now)
    {
        var action = _actions.Find(a => a.Id == actionId);
        if (action is null)
        {
            return Result<ProblemAction>.Failure(ProblemErrors.ActionNotFound);
        }

        if (action.IsClosed)
        {
            return Result<ProblemAction>.Failure(ProblemErrors.ActionAlreadyClosed);
        }

        action.Close(now);
        if (Status == ProblemStatus.Analyzed && _actions.TrueForAll(a => a.IsClosed))
        {
            Status = ProblemStatus.Resolved;
        }

        return Result<ProblemAction>.Success(action);
    }

    /// <summary>
    /// Niveau de blocage au jour <paramref name="today"/> (date métier) : immobilisant non résolu, puis signalé non
    /// analysé, puis au moins une action en retard (échéance strictement avant aujourd'hui et action non close), sinon autre.
    /// </summary>
    public ProblemBlockingLevel BlockingLevelAt(DateOnly today)
    {
        if (Status != ProblemStatus.Resolved && Immobilized == true)
        {
            return ProblemBlockingLevel.Immobilizing;
        }

        if (Status == ProblemStatus.Reported)
        {
            return ProblemBlockingLevel.Unanalyzed;
        }

        return _actions.Exists(a => !a.IsClosed && a.DueDate is { } due && due < today)
            ? ProblemBlockingLevel.OverdueAction
            : ProblemBlockingLevel.Other;
    }

    private static string? CleanComment(string? comment) => string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
}
