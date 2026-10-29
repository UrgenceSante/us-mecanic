using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Vérifie qu'une intervention liée à une action appartient au carnet d'entretien du véhicule du problème.</summary>
public sealed class ProblemActionInterventionCheck(IInterventionRepository interventions)
{
    /// <summary>Renvoie l'erreur à produire, ou <c>null</c> si aucune intervention n'est liée ou si elle est valide.</summary>
    public async Task<DomainError?> CheckAsync(Guid vehicleId, Guid? interventionId, CancellationToken cancellationToken)
    {
        if (interventionId is not { } id)
        {
            return null;
        }

        var intervention = await interventions.GetAsync(vehicleId, id, cancellationToken);
        return intervention is null ? ProblemErrors.ActionInterventionInvalid : null;
    }
}
