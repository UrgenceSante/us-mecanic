using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Maintenance;

/// <summary>
/// Intervention réalisée sur un véhicule (carnet d'entretien) : date réelle, km facultatif, garage facultatif,
/// travaux et date de saisie. Elle réalise 0..n échéances du véhicule, chacune au plus une fois.
/// </summary>
public sealed class Intervention
{
    public const int MaxWorkLength = 2000;

    private readonly List<InterventionDeadline> _deadlines = [];

    // Les paramètres portent le nom des propriétés : c'est ce qui permet à EF Core de rematérialiser l'entité.
    private Intervention(Guid id, Guid vehicleId, DateOnly date, int? kilometers, Guid? garageId, string work, DateTimeOffset recordedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        Date = date;
        Kilometers = kilometers;
        GarageId = garageId;
        Work = work;
        RecordedAt = recordedAt;
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    /// <summary>Date réelle de l'intervention (et non de sa saisie).</summary>
    public DateOnly Date { get; }

    public int? Kilometers { get; }

    public Guid? GarageId { get; }

    public string Work { get; }

    /// <summary>Date de saisie de l'intervention.</summary>
    public DateTimeOffset RecordedAt { get; }

    /// <summary>Échéances du véhicule réalisées par cette intervention.</summary>
    public IReadOnlyCollection<InterventionDeadline> Deadlines => _deadlines;

    /// <summary>
    /// Crée l'intervention : travaux obligatoires et bornés, date pas dans le futur, échéances sans doublon.
    /// L'appartenance des échéances au véhicule et le garage sont contrôlés par le cas d'usage.
    /// </summary>
    public static Result<Intervention> Create(
        Guid vehicleId, DateOnly date, int? kilometers, Guid? garageId, string? work, IReadOnlyCollection<Guid> deadlineIds, DateOnly today, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(deadlineIds);

        var cleaned = work?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<Intervention>.Failure(InterventionErrors.WorkRequired);
        }

        if (cleaned.Length > MaxWorkLength)
        {
            return Result<Intervention>.Failure(InterventionErrors.WorkTooLong);
        }

        if (date > today)
        {
            return Result<Intervention>.Failure(InterventionErrors.DateInFuture);
        }

        if (new HashSet<Guid>(deadlineIds).Count != deadlineIds.Count)
        {
            return Result<Intervention>.Failure(InterventionErrors.DeadlineDuplicated);
        }

        var id = Guid.CreateVersion7(now);
        var intervention = new Intervention(id, vehicleId, date, kilometers, garageId, cleaned, now);
        intervention._deadlines.AddRange(deadlineIds.Select(deadlineId => new InterventionDeadline(id, deadlineId)));
        return Result<Intervention>.Success(intervention);
    }
}
