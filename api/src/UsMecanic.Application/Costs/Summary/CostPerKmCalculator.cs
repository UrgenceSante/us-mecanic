using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Costs.Summary;

/// <summary>
/// Règle du coût au kilomètre, sans accès aux données. Par véhicule : km parcourus = dernier relevé − premier relevé de la
/// période (par date, puis par km à date égale) ; non calculable s'il y a moins de deux relevés ou des km parcourus ≤ 0.
/// Pour le parc : somme des dépenses et des km des seuls véhicules calculables ; les autres sont comptés comme exclus.
/// Résultat en euros par km, arrondi à 2 décimales (au plus proche, 0,005 arrondi vers le haut).
/// </summary>
public static class CostPerKmCalculator
{
    public const int Decimals = 2;

    /// <param name="lines">Toutes les dépenses de la période (emprunts compris), avec leur véhicule.</param>
    /// <param name="readings">Relevés dont la date est dans la période.</param>
    /// <param name="singleVehicle">La demande porte sur un seul véhicule : aucune exclusion n'est comptée.</param>
    public static CostPerKmDto Compute(IEnumerable<CostLine> lines, IEnumerable<MileageReading> readings, bool singleVehicle)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(readings);

        List<CostLine> costs = [.. lines];
        List<MileageReading> all = [.. readings];
        var computable = all
            .GroupBy(r => r.VehicleId)
            .Select(g => (VehicleId: g.Key, Kilometers: Travelled([.. g])))
            .Where(x => x.Kilometers > 0)
            .ToDictionary(x => x.VehicleId, x => x.Kilometers);

        var known = costs
            .Where(l => l.VehicleId.HasValue)
            .Select(l => l.VehicleId!.Value)
            .Concat(all.Select(r => r.VehicleId))
            .Distinct();
        var excluded = singleVehicle ? 0 : known.Count(id => !computable.ContainsKey(id));

        if (computable.Count == 0)
        {
            return new CostPerKmDto(0, 0m, null, false, excluded);
        }

        var kilometers = computable.Values.Sum();
        var cost = costs.Where(l => l.VehicleId is { } id && computable.ContainsKey(id)).Sum(l => l.Amount);
        var value = decimal.Round(cost / kilometers, Decimals, MidpointRounding.AwayFromZero);
        return new CostPerKmDto(kilometers, decimal.Round(cost, 2), value, true, excluded);
    }

    private static int Travelled(List<MileageReading> readings)
    {
        if (readings.Count < 2)
        {
            return 0;
        }

        var first = readings.OrderBy(r => r.Date).ThenBy(r => r.Kilometers).First();
        var last = readings.OrderByDescending(r => r.Date).ThenByDescending(r => r.Kilometers).First();
        return last.Kilometers - first.Kilometers;
    }
}
