using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

public sealed record RegisterVehicleCommand(string LicensePlate, VehicleDetailsInput Details);

/// <summary>Ajoute un véhicule au parc. L'immatriculation doit être unique.</summary>
public sealed class RegisterVehicle(IVehicleRepository repository, VehicleTypeLookup typeLookup, TimeProvider clock, BusinessCalendar calendar)
{
    public async Task<Result<VehicleDto>> ExecuteAsync(RegisterVehicleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var plate = LicensePlate.Create(command.LicensePlate);
        if (plate.IsFailure)
        {
            return Result<VehicleDto>.Failure(plate.Error!);
        }

        var details = command.Details.ToDetails();
        if (details.IsFailure)
        {
            return Result<VehicleDto>.Failure(details.Error!);
        }

        var typeError = await typeLookup.CheckAssignableAsync(details.Value.TypeId, null, cancellationToken);
        if (typeError is not null)
        {
            return Result<VehicleDto>.Failure(typeError);
        }

        if (await repository.GetByPlateAsync(plate.Value, cancellationToken) is not null)
        {
            return Result<VehicleDto>.Failure(VehicleErrors.PlateAlreadyExists);
        }

        var vehicle = Vehicle.Register(plate.Value, details.Value, calendar.Today, clock.GetUtcNow());
        if (vehicle.IsFailure)
        {
            return Result<VehicleDto>.Failure(vehicle.Error!);
        }

        repository.Add(vehicle.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<VehicleDto>.Success(await typeLookup.ToDtoAsync(vehicle.Value, cancellationToken));
    }
}
