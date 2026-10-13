using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles;

/// <summary>Critères de recherche : texte libre (plaque, marque, modèle, n° interne) et statut.</summary>
public sealed record VehicleFilter(string? Search = null, VehicleStatus? Status = null);
