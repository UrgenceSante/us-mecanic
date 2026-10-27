using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>Critères de recherche : texte libre (plaque, marque, modèle, n° interne), statut et type.</summary>
public sealed record VehicleFilter(string? Search = null, VehicleStatus? Status = null, Guid? TypeId = null);
