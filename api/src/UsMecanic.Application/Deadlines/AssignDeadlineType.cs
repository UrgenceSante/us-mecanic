using UsMecanic.Application.Mileage;
using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Types;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Deadlines;

/// <summary>
/// Ajoute un type d'échéance périodique aux véhicules d'un ou plusieurs types de véhicule, avec la périodicité
/// par défaut du type. Les véhicules sortis du parc et ceux qui ont déjà ce type en cours sont ignorés.
/// Dernière réalisation : la date fournie, sinon aujourd'hui ; le km est le km actuel du véhicule quand le type
/// a une périodicité en km (un véhicule sans relevé est alors en erreur <c>deadline_completion.kilometers_required</c>).
/// Une seule sauvegarde pour tout le lot.
/// </summary>
public sealed class AssignDeadlineType(
    IDeadlineTypeRepository types,
    IVehicleTypeRepository vehicleTypes,
    IVehicleRepository vehicles,
    IVehicleDeadlineRepository deadlines,
    IMileageReadingRepository mileage,
    BusinessCalendar calendar)
{
    public async Task<Result<AssignDeadlineTypeResult>> ExecuteAsync(
        Guid deadlineTypeId,
        IReadOnlyCollection<Guid>? vehicleTypeIds,
        DateOnly? lastDoneDate,
        CancellationToken cancellationToken)
    {
        var ids = vehicleTypeIds is null ? new List<Guid>() : vehicleTypeIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Failure(VehicleDeadlineErrors.VehicleTypesRequired);
        }

        var type = await types.GetByIdAsync(deadlineTypeId, cancellationToken);
        if (type is null)
        {
            return Failure(DeadlineTypeErrors.NotFound);
        }

        var today = calendar.Today;
        var precondition = Precondition(type, lastDoneDate, today);
        if (precondition is not null)
        {
            return Failure(precondition);
        }

        foreach (var id in ids)
        {
            if (await vehicleTypes.GetByIdAsync(id, cancellationToken) is null)
            {
                return Failure(VehicleTypeErrors.Unknown);
            }
        }

        var targets = new List<Vehicle>();
        foreach (var id in ids)
        {
            targets.AddRange(await vehicles.ListAsync(new VehicleFilter(TypeId: id), cancellationToken));
        }

        var result = await AssignAsync(type, targets, lastDoneDate ?? today, today, cancellationToken);
        return Result<AssignDeadlineTypeResult>.Success(result);
    }

    private static Result<AssignDeadlineTypeResult> Failure(DomainError error) => Result<AssignDeadlineTypeResult>.Failure(error);

    private static DomainError? Precondition(DeadlineType type, DateOnly? lastDoneDate, DateOnly today)
    {
        if (!type.IsActive)
        {
            return VehicleDeadlineErrors.TypeInactive;
        }

        if (type.IsOneOff)
        {
            return VehicleDeadlineErrors.OneOffNotAssignable;
        }

        return lastDoneDate > today ? VehicleDeadlineErrors.LastDoneInFuture : null;
    }

    private async Task<AssignDeadlineTypeResult> AssignAsync(
        DeadlineType type,
        List<Vehicle> targets,
        DateOnly lastDoneDate,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var retired = targets.Count(v => v.Status == VehicleStatus.Retired);
        var usable = targets.Where(v => v.Status != VehicleStatus.Retired).ToList();

        var activeVehicleIds = (await deadlines.ListActiveAsync(null, type.Id, cancellationToken)).Select(d => d.VehicleId).ToHashSet();
        var candidates = usable.Where(v => !activeVehicleIds.Contains(v.Id)).ToList();
        var currents = type.DefaultIntervalKm is null
            ? new Dictionary<Guid, CurrentMileage>()
            : await mileage.GetCurrentByVehicleAsync([.. candidates.Select(v => v.Id)], cancellationToken);

        var added = 0;
        var errors = new List<AssignDeadlineTypeError>();
        foreach (var vehicle in candidates)
        {
            int? lastDoneKm = currents.TryGetValue(vehicle.Id, out var current) ? current.Kilometers : null;
            var schedule = new VehicleDeadlineSchedule(type.DefaultIntervalMonths, type.DefaultIntervalKm, lastDoneDate, lastDoneKm, null, null);
            var created = type.DefaultIntervalKm is not null && lastDoneKm is null
                ? Result<VehicleDeadline>.Failure(DeadlineCompletionErrors.KilometersRequired)
                : VehicleDeadline.Create(vehicle.Id, type, schedule, today);
            if (created.IsFailure)
            {
                var error = created.Error!;
                errors.Add(new AssignDeadlineTypeError(vehicle.Id, vehicle.LicensePlate.Value, error.Code, error.Message));
                continue;
            }

            deadlines.Add(created.Value);
            added++;
        }

        if (added > 0)
        {
            await deadlines.SaveChangesAsync(cancellationToken);
        }

        return new AssignDeadlineTypeResult(added, usable.Count - candidates.Count, retired, errors);
    }
}
