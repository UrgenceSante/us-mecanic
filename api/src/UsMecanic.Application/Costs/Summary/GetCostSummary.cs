using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Summary;

/// <summary>Synthèse des coûts d'un véhicule ou du parc, par mois ou par année.</summary>
public sealed class GetCostSummary(MileageVehicleGuard guard, CostLineReader reader, IMileageReadingRepository readings)
{
    public async Task<Result<CostSummaryDto>> ExecuteAsync(CostSummaryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invalid = Validate(query, out var granularity);
        if (invalid is not null)
        {
            return Result<CostSummaryDto>.Failure(invalid);
        }

        var from = query.From!.Value;
        var to = query.To!.Value;
        if (query.VehicleId is { } vehicleId)
        {
            // Un véhicule sorti du parc reste consultable : ses coûts font partie de l'historique.
            var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
            if (error is not null)
            {
                return Result<CostSummaryDto>.Failure(error);
            }
        }

        var lines = await reader.ReadAsync(from, to, query.VehicleId, cancellationToken);
        var inPeriod = await readings.ListInPeriodAsync(from, to, query.VehicleId, cancellationToken);
        var costPerKm = CostPerKmCalculator.Compute(lines, inPeriod, query.VehicleId.HasValue);
        return Result<CostSummaryDto>.Success(CostAggregator.Build(from, to, granularity, query.VehicleId, lines, costPerKm));
    }

    private static DomainError? Validate(CostSummaryQuery query, out CostGranularity granularity)
    {
        granularity = CostGranularity.Month;
        if (query.From is not { } from)
        {
            return CostSummaryErrors.FromRequired;
        }

        if (query.To is not { } to)
        {
            return CostSummaryErrors.ToRequired;
        }

        if (from > to)
        {
            return CostSummaryErrors.InvalidRange;
        }

        if (from.AddYears(CostSummaryErrors.MaxYears) < to)
        {
            return CostSummaryErrors.RangeTooLong;
        }

        var requested = query.Granularity?.Trim();
        if (string.IsNullOrEmpty(requested) || string.Equals(requested, "month", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (string.Equals(requested, "year", StringComparison.OrdinalIgnoreCase))
        {
            granularity = CostGranularity.Year;
            return null;
        }

        return CostSummaryErrors.InvalidGranularity;
    }
}
