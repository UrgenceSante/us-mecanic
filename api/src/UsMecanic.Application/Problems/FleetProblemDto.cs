using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Véhicule d'une ligne de la liste des problèmes du parc.</summary>
public sealed record FleetProblemVehicleDto(Guid Id, string LicensePlate);

/// <summary>Nature (référentiel) d'une ligne de la liste des problèmes du parc.</summary>
public sealed record FleetProblemNatureDto(Guid Id, string Name);

/// <summary>
/// Ligne de la liste des problèmes du parc : le problème, son véhicule, sa nature (nulle tant qu'il n'est pas analysé)
/// et son niveau de blocage calculé au jour métier.
/// </summary>
public sealed record FleetProblemDto(
    ProblemDto Problem,
    FleetProblemVehicleDto Vehicle,
    FleetProblemNatureDto? Nature,
    ProblemBlockingLevel BlockingLevel);
