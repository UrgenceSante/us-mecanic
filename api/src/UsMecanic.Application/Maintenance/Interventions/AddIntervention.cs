using UsMecanic.Application.Deadlines;
using UsMecanic.Application.Maintenance.Garages;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>
/// Ajoute une intervention au carnet. Chaque échéance cochée est réalisée (<see cref="VehicleDeadline.Complete"/>) à la date
/// et au km de l'intervention ; un km fourni crée un relevé avec les mêmes contrôles que <see cref="AddMileageReading"/>.
/// Intervention, liens, relevé et échéances sont enregistrés ensemble (une seule transaction) : rien n'est écrit en cas d'erreur.
/// </summary>
public sealed class AddIntervention(
    MileageVehicleGuard guard,
    IGarageRepository garages,
    IVehicleDeadlineRepository deadlines,
    IMileageReadingRepository readings,
    IInterventionRepository interventions,
    InterventionDtoFactory factory,
    TimeProvider clock,
    BusinessCalendar calendar)
{
    public async Task<Result<InterventionDto>> ExecuteAsync(Guid vehicleId, InterventionInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<InterventionDto>.Failure(error);
        }

        var now = clock.GetUtcNow();
        var today = calendar.Today;
        var created = Intervention.Create(
            vehicleId, input.Date, input.Kilometers, input.GarageId, input.Work, input.DeadlineIds ?? [], today, now, input.Amount);
        if (created.IsFailure)
        {
            return Result<InterventionDto>.Failure(created.Error!);
        }

        var intervention = created.Value;
        var garageError = await CheckGarageAsync(input.GarageId, cancellationToken);
        if (garageError is not null)
        {
            return Result<InterventionDto>.Failure(garageError);
        }

        var targets = await LoadDeadlinesAsync(vehicleId, intervention, cancellationToken);
        if (targets.IsFailure)
        {
            return Result<InterventionDto>.Failure(targets.Error!);
        }

        var reading = await CreateReadingAsync(vehicleId, input, today, now, cancellationToken);
        if (reading.IsFailure)
        {
            return Result<InterventionDto>.Failure(reading.Error!);
        }

        // Le km est obligatoire dès qu'une échéance cochée a un critère km. Contrôlé avant de réaliser la moindre
        // échéance pour n'en modifier aucune en cas d'erreur (VehicleDeadline.Complete applique la même règle).
        if (input.Kilometers is null && targets.Value.Any(d => d.IntervalKm is not null || d.DueKm is not null))
        {
            return Result<InterventionDto>.Failure(DeadlineCompletionErrors.KilometersRequired);
        }

        foreach (var deadline in targets.Value)
        {
            var done = deadline.Complete(input.Date, input.Kilometers, today);
            if (done.IsFailure)
            {
                return Result<InterventionDto>.Failure(done.Error!);
            }
        }

        if (reading.Value is not null)
        {
            readings.Add(reading.Value);
        }

        interventions.Add(intervention);
        await interventions.SaveChangesAsync(cancellationToken);
        return Result<InterventionDto>.Success((await factory.BuildAsync(vehicleId, [intervention], cancellationToken))[0]);
    }

    private async Task<DomainError?> CheckGarageAsync(Guid? garageId, CancellationToken cancellationToken)
    {
        if (garageId is not { } id)
        {
            return null;
        }

        var garage = await garages.GetByIdAsync(id, cancellationToken);
        if (garage is null)
        {
            return InterventionErrors.GarageUnknown;
        }

        return garage.IsActive ? null : InterventionErrors.GarageInactive;
    }

    /// <summary>Échéances cochées : elles doivent appartenir au véhicule et ne pas être closes.</summary>
    private async Task<Result<IReadOnlyList<VehicleDeadline>>> LoadDeadlinesAsync(
        Guid vehicleId, Intervention intervention, CancellationToken cancellationToken)
    {
        if (intervention.Deadlines.Count == 0)
        {
            return Result<IReadOnlyList<VehicleDeadline>>.Success([]);
        }

        var ofVehicle = (await deadlines.ListByVehicleAsync(vehicleId, cancellationToken)).ToDictionary(d => d.Id);
        var found = new List<VehicleDeadline>();
        foreach (var link in intervention.Deadlines)
        {
            if (!ofVehicle.TryGetValue(link.VehicleDeadlineId, out var deadline))
            {
                return Result<IReadOnlyList<VehicleDeadline>>.Failure(InterventionErrors.DeadlineUnknown);
            }

            if (deadline.IsClosed)
            {
                return Result<IReadOnlyList<VehicleDeadline>>.Failure(InterventionErrors.DeadlineClosed);
            }

            found.Add(deadline);
        }

        return Result<IReadOnlyList<VehicleDeadline>>.Success(found);
    }

    /// <summary>Relevé à créer (mêmes contrôles qu'un ajout de relevé), ou <c>null</c> sans km.</summary>
    private async Task<Result<MileageReading?>> CreateReadingAsync(
        Guid vehicleId, InterventionInput input, DateOnly today, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (input.Kilometers is not { } kilometers)
        {
            return Result<MileageReading?>.Success(null);
        }

        var existing = await readings.ListByVehicleAsync(vehicleId, cancellationToken);
        var entry = new MileageReadingEntry(input.Date, kilometers, input.Reason, input.ConfirmHighMileage);
        var created = MileageReading.Create(vehicleId, entry, existing, today, now);
        return created.IsSuccess
            ? Result<MileageReading?>.Success(created.Value)
            : Result<MileageReading?>.Failure(created.Error!);
    }
}
