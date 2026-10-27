using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

/// <summary>
/// Seuils globaux « échéance proche », appliqués aux types qui n'ont pas les leurs.
/// Il n'existe qu'une seule ligne, d'identifiant <see cref="SingletonId"/>.
/// </summary>
public sealed class DeadlineThresholds
{
    public const int DefaultDays = 30;
    public const int DefaultKm = 1000;

    public static readonly Guid SingletonId = new("0d1a5e00-0000-4000-8000-000000000001");

    private DeadlineThresholds(Guid id, int dueSoonDays, int dueSoonKm)
    {
        Id = id;
        DueSoonDays = dueSoonDays;
        DueSoonKm = dueSoonKm;
    }

    public Guid Id { get; }

    public int DueSoonDays { get; private set; }

    public int DueSoonKm { get; private set; }

    public static DeadlineThresholds CreateDefault() => new(SingletonId, DefaultDays, DefaultKm);

    /// <summary>Modifie les deux seuils (obligatoires, strictement positifs) ; rien n'est modifié en cas d'erreur.</summary>
    public Result<DeadlineThresholds> Update(int? dueSoonDays, int? dueSoonKm)
    {
        if (dueSoonDays is null or <= 0)
        {
            return Result<DeadlineThresholds>.Failure(DeadlineThresholdsErrors.DaysInvalid);
        }

        if (dueSoonKm is null or <= 0)
        {
            return Result<DeadlineThresholds>.Failure(DeadlineThresholdsErrors.KmInvalid);
        }

        DueSoonDays = dueSoonDays.Value;
        DueSoonKm = dueSoonKm.Value;
        return Result<DeadlineThresholds>.Success(this);
    }
}
