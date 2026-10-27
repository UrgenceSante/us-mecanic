using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Dashboard;

/// <summary>Véhicule d'une ligne du tableau de bord ; le type de véhicule est facultatif.</summary>
public sealed record DashboardVehicleDto(Guid Id, string LicensePlate, string? InternalNumber, Guid? TypeId, string? TypeName);

/// <summary>Type d'échéance d'une ligne du tableau de bord.</summary>
public sealed record DashboardDeadlineTypeDto(Guid Id, string Name);

/// <summary>Échéance proche ou dépassée du parc, avec le km actuel du véhicule (nul sans relevé).</summary>
public sealed record DashboardDeadlineDto(
    Guid Id,
    DashboardVehicleDto Vehicle,
    DashboardDeadlineTypeDto DeadlineType,
    DeadlineStatus Status,
    DateOnly? DueDate,
    int? DueKm,
    int? CurrentKm);
