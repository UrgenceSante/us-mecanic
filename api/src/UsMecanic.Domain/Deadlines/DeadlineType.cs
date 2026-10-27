using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

/// <summary>
/// Type d'échéance administrable (contrôle technique, assurance, agrément ARS…). Jamais supprimé,
/// seulement désactivé, pour rester affiché sur les échéances qui l'utilisent.
/// </summary>
public sealed class DeadlineType
{
    public const int MaxNameLength = 100;

    private DeadlineType(Guid id, string name, int? defaultIntervalMonths, int? defaultIntervalKm, int? dueSoonDays, int? dueSoonKm, bool isActive)
    {
        Id = id;
        Name = name;
        DefaultIntervalMonths = defaultIntervalMonths;
        DefaultIntervalKm = defaultIntervalKm;
        DueSoonDays = dueSoonDays;
        DueSoonKm = dueSoonKm;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    /// <summary>Périodicité par défaut en mois ; nulle si le type n'en a pas.</summary>
    public int? DefaultIntervalMonths { get; private set; }

    /// <summary>Périodicité par défaut en kilomètres ; nulle si le type n'en a pas.</summary>
    public int? DefaultIntervalKm { get; private set; }

    /// <summary>Seuil « proche » en jours propre au type ; nul = seuil global.</summary>
    public int? DueSoonDays { get; private set; }

    /// <summary>Seuil « proche » en kilomètres propre au type ; nul = seuil global.</summary>
    public int? DueSoonKm { get; private set; }

    /// <summary>Un type inactif n'est plus proposé pour de nouvelles échéances.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Un type sans périodicité par défaut est ponctuel (non renouvelé automatiquement).</summary>
    public bool IsOneOff => DefaultIntervalMonths is null && DefaultIntervalKm is null;

    public static Result<DeadlineType> Create(string? name, DeadlineTypeSettings? settings)
    {
        var validated = Validate(name, settings);
        if (validated.IsFailure)
        {
            return Result<DeadlineType>.Failure(validated.Error!);
        }

        var s = settings ?? new DeadlineTypeSettings(null, null, null, null);
        return Result<DeadlineType>.Success(
            new DeadlineType(Guid.CreateVersion7(), validated.Value, s.IntervalMonths, s.IntervalKm, s.DueSoonDays, s.DueSoonKm, true));
    }

    /// <summary>Modifie le nom, la périodicité et les seuils ; rien n'est modifié en cas d'erreur.</summary>
    public Result<DeadlineType> Update(string? name, DeadlineTypeSettings? settings)
    {
        var validated = Validate(name, settings);
        if (validated.IsFailure)
        {
            return Result<DeadlineType>.Failure(validated.Error!);
        }

        var s = settings ?? new DeadlineTypeSettings(null, null, null, null);
        Name = validated.Value;
        DefaultIntervalMonths = s.IntervalMonths;
        DefaultIntervalKm = s.IntervalKm;
        DueSoonDays = s.DueSoonDays;
        DueSoonKm = s.DueSoonKm;
        return Result<DeadlineType>.Success(this);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>Valide les données d'un type et renvoie le nom nettoyé : nom obligatoire, valeurs définies strictement positives.</summary>
    public static Result<string> Validate(string? name, DeadlineTypeSettings? settings)
    {
        var cleaned = name?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<string>.Failure(DeadlineTypeErrors.NameRequired);
        }

        if (cleaned.Length > MaxNameLength)
        {
            return Result<string>.Failure(DeadlineTypeErrors.NameTooLong);
        }

        var error = FirstSettingsError(settings ?? new DeadlineTypeSettings(null, null, null, null));
        return error is null ? Result<string>.Success(cleaned) : Result<string>.Failure(error);
    }

    private static DomainError? FirstSettingsError(DeadlineTypeSettings s)
    {
        if (s.IntervalMonths <= 0)
        {
            return DeadlineTypeErrors.IntervalMonthsInvalid;
        }

        if (s.IntervalKm <= 0)
        {
            return DeadlineTypeErrors.IntervalKmInvalid;
        }

        if (s.DueSoonDays <= 0)
        {
            return DeadlineTypeErrors.DueSoonDaysInvalid;
        }

        return s.DueSoonKm <= 0 ? DeadlineTypeErrors.DueSoonKmInvalid : null;
    }
}
