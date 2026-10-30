namespace UsMecanic.Application.Maintenance.Interventions;

/// <summary>Intervention du carnet d'entretien, avec son garage et les échéances qu'elle a réalisées.</summary>
public sealed record InterventionDto(
    Guid Id,
    Guid VehicleId,
    DateOnly Date,
    int? Kilometers,
    InterventionGarageDto? Garage,
    string Work,
    DateTimeOffset RecordedAt,
    decimal? Amount,
    IReadOnlyList<InterventionDeadlineDto> Deadlines);

public sealed record InterventionGarageDto(Guid Id, string Name);

/// <summary>Échéance réalisée : identifiant et nom de son type (<c>null</c> si le type est introuvable).</summary>
public sealed record InterventionDeadlineDto(Guid Id, string? TypeName);
