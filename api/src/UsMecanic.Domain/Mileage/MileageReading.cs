using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Mileage;

/// <summary>
/// Relevé kilométrique d'un véhicule à une date. Le km actuel d'un véhicule est celui du relevé
/// le plus récent par date. L'auteur n'est pas enregistré en phase 2.
/// </summary>
public sealed class MileageReading
{
    public const int MaxReasonLength = 500;

    private MileageReading(Guid id, Guid vehicleId, DateOnly date, int kilometers, string? reason, DateTimeOffset recordedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        Date = date;
        Kilometers = kilometers;
        Reason = reason;
        RecordedAt = recordedAt;
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    /// <summary>Date du relevé (et non de sa saisie).</summary>
    public DateOnly Date { get; private set; }

    public int Kilometers { get; private set; }

    /// <summary>Motif facultatif ; obligatoire quand la valeur est incohérente avec les relevés voisins.</summary>
    public string? Reason { get; private set; }

    /// <summary>Date de saisie du relevé (inchangée par une modification).</summary>
    public DateTimeOffset RecordedAt { get; }

    public static Result<MileageReading> Create(Guid vehicleId, MileageReadingEntry entry, IEnumerable<MileageReading> others, DateOnly today, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(others);

        var validated = Validate(entry, others, today);
        return validated.IsSuccess
            ? Result<MileageReading>.Success(new MileageReading(
                Guid.CreateVersion7(now), vehicleId, validated.Value.Date, validated.Value.Kilometers, validated.Value.Reason, now))
            : Result<MileageReading>.Failure(validated.Error!);
    }

    /// <summary>Applique les mêmes contrôles que la création, en excluant ce relevé des voisins.</summary>
    public Result<MileageReading> Update(MileageReadingEntry entry, IEnumerable<MileageReading> others, DateOnly today, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(others);

        var validated = Validate(entry, others.Where(r => r.Id != Id), today);
        if (validated.IsFailure)
        {
            return Result<MileageReading>.Failure(validated.Error!);
        }

        Date = validated.Value.Date;
        Kilometers = validated.Value.Kilometers;
        Reason = validated.Value.Reason;
        return Result<MileageReading>.Success(this);
    }

    private static Result<MileageReadingEntry> Validate(MileageReadingEntry entry, IEnumerable<MileageReading> others, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var reason = string.IsNullOrWhiteSpace(entry.Reason) ? null : entry.Reason.Trim();
        if (reason?.Length > MaxReasonLength)
        {
            return Result<MileageReadingEntry>.Failure(MileageReadingErrors.ReasonTooLong);
        }

        var cleaned = entry with { Reason = reason };
        var error = MileageConsistency.Check(cleaned, others, today);
        return error is null
            ? Result<MileageReadingEntry>.Success(cleaned)
            : Result<MileageReadingEntry>.Failure(error);
    }
}
