using UsMecanic.Application.Mileage;
using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Signale un problème sur un véhicule désigné par son immatriculation. Le problème est créé au statut « Signalé ».
/// Un véhicule inconnu est refusé ; un véhicule sorti du parc l'est aussi (garde partagée).
/// </summary>
public sealed class ReportProblem(
    IVehicleRepository vehicles,
    MileageVehicleGuard guard,
    IProblemRepository problems,
    TimeProvider clock,
    BusinessCalendar calendar)
{
    public async Task<Result<ProblemDto>> ExecuteAsync(ReportProblemInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var plate = LicensePlate.Create(input.LicensePlate);
        if (plate.IsFailure)
        {
            return Result<ProblemDto>.Failure(plate.Error!);
        }

        var vehicle = await vehicles.GetByPlateAsync(plate.Value, cancellationToken);
        if (vehicle is null)
        {
            return Result<ProblemDto>.Failure(ProblemErrors.VehicleUnknown);
        }

        var error = await guard.CheckAsync(vehicle.Id, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<ProblemDto>.Failure(error);
        }

        var created = Problem.Create(vehicle.Id, input.Date, input.Description, input.Author, calendar.Today, clock.GetUtcNow());
        if (created.IsFailure)
        {
            return Result<ProblemDto>.Failure(created.Error!);
        }

        problems.Add(created.Value);
        await problems.SaveChangesAsync(cancellationToken);
        return Result<ProblemDto>.Success(ProblemDto.From(created.Value));
    }
}
