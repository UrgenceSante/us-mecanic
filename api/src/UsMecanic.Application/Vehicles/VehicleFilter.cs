using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>
/// Critères de recherche : texte libre (plaque, marque, modèle, n° interne), statut, type, société propriétaire
/// (<c>OwnerMissing</c> : véhicules sans propriétaire) et société utilisatrice du jour (règle R4). Les critères se combinent (ET).
/// </summary>
public sealed record VehicleFilter(
    string? Search = null,
    VehicleStatus? Status = null,
    Guid? TypeId = null,
    Guid? OwnerCompanyId = null,
    bool OwnerMissing = false,
    Guid? CurrentUserCompanyId = null);
