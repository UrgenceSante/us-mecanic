using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Deadlines;

/// <summary>
/// Échéance d'un véhicule (contrôle technique, assurance, entretien…) : un type, une périodicité propre
/// (préremplie depuis le type, modifiable), la dernière réalisation et la prochaine échéance en date et/ou km.
/// Un même type ne peut être actif (non clos) qu'une fois par véhicule.
/// </summary>
public sealed class VehicleDeadline
{
    public const int MaxIntervalMonths = 1200;
    public const int MaxKilometers = 10_000_000;

    // Les paramètres portent le nom des propriétés : c'est ce qui permet à EF Core de rematérialiser l'entité.
    private VehicleDeadline(
        Guid id,
        Guid vehicleId,
        Guid deadlineTypeId,
        int? intervalMonths,
        int? intervalKm,
        DateOnly? lastDoneDate,
        int? lastDoneKm,
        DateOnly? dueDate,
        int? dueKm)
    {
        Id = id;
        VehicleId = vehicleId;
        DeadlineTypeId = deadlineTypeId;
        IntervalMonths = intervalMonths;
        IntervalKm = intervalKm;
        LastDoneDate = lastDoneDate;
        LastDoneKm = lastDoneKm;
        DueDate = dueDate;
        DueKm = dueKm;
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    public Guid DeadlineTypeId { get; }

    /// <summary>Périodicité propre à cette échéance, en mois ; nulle si absente.</summary>
    public int? IntervalMonths { get; private set; }

    /// <summary>Périodicité propre à cette échéance, en kilomètres ; nulle si absente.</summary>
    public int? IntervalKm { get; private set; }

    public DateOnly? LastDoneDate { get; private set; }

    public int? LastDoneKm { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public int? DueKm { get; private set; }

    /// <summary>Une échéance close n'est plus suivie (ponctuelle réalisée).</summary>
    public bool IsClosed { get; private set; }

    public VehicleDeadlineSchedule Schedule => new(IntervalMonths, IntervalKm, LastDoneDate, LastDoneKm, DueDate, DueKm);

    public static Result<VehicleDeadline> Create(Guid vehicleId, DeadlineType type, VehicleDeadlineSchedule schedule, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(schedule);

        if (!type.IsActive)
        {
            return Result<VehicleDeadline>.Failure(VehicleDeadlineErrors.TypeInactive);
        }

        var resolved = Resolve(schedule, today);
        return resolved.IsSuccess
            ? Result<VehicleDeadline>.Success(NewFrom(vehicleId, type.Id, resolved.Value))
            : Result<VehicleDeadline>.Failure(resolved.Error!);
    }

    /// <summary>
    /// Modifie la périodicité et recalcule la prochaine échéance depuis la dernière réalisation.
    /// Une périodicité vide rend l'échéance ponctuelle : date et km d'échéance actuels sont conservés.
    /// Rien n'est modifié en cas d'erreur.
    /// </summary>
    public Result<VehicleDeadline> ChangeInterval(int? intervalMonths, int? intervalKm, DateOnly today)
    {
        if (IsClosed)
        {
            return Result<VehicleDeadline>.Failure(VehicleDeadlineErrors.Closed);
        }

        var resolved = Resolve(Schedule with { IntervalMonths = intervalMonths, IntervalKm = intervalKm }, today);
        if (resolved.IsFailure)
        {
            return Result<VehicleDeadline>.Failure(resolved.Error!);
        }

        IntervalMonths = resolved.Value.IntervalMonths;
        IntervalKm = resolved.Value.IntervalKm;
        DueDate = resolved.Value.DueDate;
        DueKm = resolved.Value.DueKm;
        return Result<VehicleDeadline>.Success(this);
    }

    /// <summary>
    /// Marque l'échéance réalisée à la date (et au km) réels. Périodique : la dernière réalisation est remplacée et
    /// la prochaine échéance recalculée depuis elle. Ponctuelle : l'échéance est close, aucune nouvelle n'est créée.
    /// Rien n'est modifié en cas d'erreur.
    /// </summary>
    public Result<VehicleDeadline> Complete(DateOnly date, int? kilometers, DateOnly today)
    {
        if (IsClosed)
        {
            return Result<VehicleDeadline>.Failure(VehicleDeadlineErrors.Closed);
        }

        if (date > today)
        {
            return Result<VehicleDeadline>.Failure(DeadlineCompletionErrors.DateInFuture);
        }

        if (kilometers is < 0 or > MaxKilometers)
        {
            return Result<VehicleDeadline>.Failure(VehicleDeadlineErrors.KilometersInvalid);
        }

        if (!Schedule.IsPeriodic)
        {
            LastDoneDate = date;
            LastDoneKm = kilometers;
            IsClosed = true;
            return Result<VehicleDeadline>.Success(this);
        }

        var resolved = Resolve(Schedule with { LastDoneDate = date, LastDoneKm = kilometers }, today);
        if (resolved.IsFailure)
        {
            return Result<VehicleDeadline>.Failure(resolved.Error!);
        }

        LastDoneDate = date;
        LastDoneKm = kilometers;
        DueDate = resolved.Value.DueDate;
        DueKm = resolved.Value.DueKm;
        return Result<VehicleDeadline>.Success(this);
    }

    /// <summary>Statut au jour donné, ou <c>null</c> si l'échéance est close.</summary>
    public DeadlineStatus? StatusAt(int? currentKm, DateOnly today, DueSoonLimits limits) =>
        IsClosed ? null : DeadlineStatusCalculator.Compute(DueDate, DueKm, currentKm, today, limits);

    private static VehicleDeadline NewFrom(Guid vehicleId, Guid deadlineTypeId, VehicleDeadlineSchedule s) =>
        new(Guid.CreateVersion7(), vehicleId, deadlineTypeId, s.IntervalMonths, s.IntervalKm, s.LastDoneDate, s.LastDoneKm, s.DueDate, s.DueKm);

    private static Result<VehicleDeadlineSchedule> Resolve(VehicleDeadlineSchedule schedule, DateOnly today)
    {
        var error = FirstError(schedule, today);
        if (error is not null)
        {
            return Result<VehicleDeadlineSchedule>.Failure(error);
        }

        var dueDate = schedule.DueDate;
        var dueKm = schedule.DueKm;
        if (schedule.IsPeriodic)
        {
            dueDate = schedule.IntervalMonths is { } months && schedule.LastDoneDate is { } doneDate ? doneDate.AddMonths(months) : null;
            dueKm = schedule.IntervalKm is { } interval && schedule.LastDoneKm is { } doneKm ? doneKm + interval : null;
        }

        return dueDate is null && dueKm is null
            ? Result<VehicleDeadlineSchedule>.Failure(VehicleDeadlineErrors.CriterionRequired)
            : Result<VehicleDeadlineSchedule>.Success(schedule with { DueDate = dueDate, DueKm = dueKm });
    }

    private static DomainError? FirstError(VehicleDeadlineSchedule s, DateOnly today)
    {
        if (s.IntervalMonths is <= 0 or > MaxIntervalMonths)
        {
            return VehicleDeadlineErrors.IntervalMonthsInvalid;
        }

        if (s.IntervalKm is <= 0 or > MaxKilometers)
        {
            return VehicleDeadlineErrors.IntervalKmInvalid;
        }

        if (s.LastDoneKm is < 0 or > MaxKilometers || s.DueKm is < 0 or > MaxKilometers)
        {
            return VehicleDeadlineErrors.KilometersInvalid;
        }

        return s.LastDoneDate > today ? VehicleDeadlineErrors.LastDoneInFuture : null;
    }
}
