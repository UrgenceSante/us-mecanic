using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Critères de la liste des problèmes du parc, combinables. Sans statut, les problèmes résolus sont exclus ; avec un
/// statut explicite (y compris « Résolu »), seuls les problèmes de ce statut sont renvoyés. <c>CompanyId</c> : véhicules
/// dont la société est propriétaire ou utilisatrice du jour.
/// </summary>
public sealed record FleetProblemFilter(ProblemStatus? Status = null, Guid? VehicleId = null, Guid? NatureId = null, Guid? CompanyId = null);
