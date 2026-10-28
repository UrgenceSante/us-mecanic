namespace UsMecanic.Application.Deadlines;

/// <summary>Bilan d'un ajout en lot d'un type d'échéance.</summary>
/// <param name="Added">Échéances créées.</param>
/// <param name="SkippedAlreadyActive">Véhicules ignorés car ils ont déjà ce type en cours (non clos).</param>
/// <param name="SkippedRetired">Véhicules ignorés car sortis du parc.</param>
/// <param name="Errors">Véhicules en erreur, avec le motif ; aucune échéance n'est créée pour eux.</param>
public sealed record AssignDeadlineTypeResult(
    int Added,
    int SkippedAlreadyActive,
    int SkippedRetired,
    IReadOnlyList<AssignDeadlineTypeError> Errors);

/// <summary>Échec pour un véhicule : code d'erreur métier stable et message.</summary>
public sealed record AssignDeadlineTypeError(Guid VehicleId, string LicensePlate, string Code, string Message);
