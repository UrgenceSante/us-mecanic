using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Deadlines;

/// <summary>
/// Liste les échéances d'un véhicule (consultable même hors parc) : en cours d'abord, par urgence puis par
/// date d'échéance ; les échéances closes à la fin.
/// </summary>
public sealed class ListVehicleDeadlines(MileageVehicleGuard guard, IVehicleDeadlineRepository deadlines, VehicleDeadlineDtoFactory factory)
{
    public async Task<Result<IReadOnlyList<VehicleDeadlineDto>>> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<VehicleDeadlineDto>>.Failure(error);
        }

        var list = await deadlines.ListByVehicleAsync(vehicleId, cancellationToken);
        var dtos = await factory.BuildAsync(vehicleId, list, cancellationToken);
        return Result<IReadOnlyList<VehicleDeadlineDto>>.Success([.. dtos
            .OrderBy(d => d.IsClosed)
            .ThenByDescending(d => d.Status)
            .ThenBy(d => d.DueDate ?? DateOnly.MaxValue)]);
    }
}
