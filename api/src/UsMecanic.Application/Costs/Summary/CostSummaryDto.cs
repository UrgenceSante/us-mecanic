namespace UsMecanic.Application.Costs.Summary;

/// <summary>Total d'une famille (toujours présente dans la réponse, zéro compris).</summary>
public sealed record CostFamilyTotal(CostFamily Family, decimal Total);

/// <summary>Total d'une catégorie au sein d'une famille.</summary>
public sealed record CostCategoryTotal(CostFamily Family, Guid? CategoryId, string Category, decimal Total);

/// <summary>Coûts d'une période (mois ou année). <c>Period</c> : « yyyy-MM » ou « yyyy » ; <c>Start</c> : premier jour de la période.</summary>
public sealed record CostPeriodDto(
    string Period,
    DateOnly Start,
    decimal Total,
    IReadOnlyList<CostFamilyTotal> Families,
    IReadOnlyList<CostCategoryTotal> Categories);

/// <summary>
/// Coût au kilomètre sur la période demandée. <c>Value</c> : €/km arrondi à 2 décimales, <c>null</c> si non calculable
/// (jamais 0 ni erreur). <c>Kilometers</c> et <c>Cost</c> : km parcourus et dépenses (emprunts compris) des seuls véhicules
/// calculables. <c>ExcludedVehicles</c> : véhicules du parc avec des dépenses ou des relevés mais non calculables
/// (toujours 0 pour une demande sur un seul véhicule : lire <c>Computable</c>).
/// </summary>
public sealed record CostPerKmDto(int Kilometers, decimal Cost, decimal? Value, bool Computable, int ExcludedVehicles);

/// <summary>Synthèse des coûts : total global de la période demandée, ventilations globales, puis une entrée par période.</summary>
public sealed record CostSummaryDto(
    DateOnly From,
    DateOnly To,
    CostGranularity Granularity,
    Guid? VehicleId,
    decimal Total,
    IReadOnlyList<CostFamilyTotal> Families,
    IReadOnlyList<CostCategoryTotal> Categories,
    IReadOnlyList<CostPeriodDto> Periods,
    CostPerKmDto CostPerKm);
