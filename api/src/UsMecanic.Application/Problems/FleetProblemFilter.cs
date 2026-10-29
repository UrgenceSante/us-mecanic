using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Critères de la liste des problèmes du parc, combinables. Sans statut, les problèmes résolus sont exclus ; avec un
/// statut explicite (y compris « Résolu »), seuls les problèmes de ce statut sont renvoyés.
/// </summary>
public sealed record FleetProblemFilter(ProblemStatus? Status = null, Guid? VehicleId = null, Guid? NatureId = null);
