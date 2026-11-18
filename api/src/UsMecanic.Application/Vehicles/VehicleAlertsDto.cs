namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Alertes d'un véhicule, calculées en lecture seule pour la liste du parc :
/// nombre d'échéances non closes dépassées et présence d'un problème immobilisant non résolu.
/// </summary>
public sealed record VehicleAlertsDto(int OverdueDeadlines, bool ImmobilizingProblem);
