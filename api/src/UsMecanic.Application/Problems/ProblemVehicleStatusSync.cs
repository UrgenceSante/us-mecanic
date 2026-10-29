using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Répercute sur le véhicule la résolution ou la réouverture d'un problème immobilisant : le véhicule repasse « en
/// service » quand son dernier problème immobilisant ouvert est résolu, et « immobilisé » quand un problème
/// immobilisant est rouvert. Un véhicule sorti du parc n'est jamais modifié. Ne sauvegarde pas : l'appelant le fait,
/// pour que problème et véhicule soient écrits dans la même transaction.
/// </summary>
public sealed class ProblemVehicleStatusSync(IProblemRepository problems, IVehicleRepository vehicles)
{
    /// <param name="problem">Problème après modification.</param>
    /// <param name="previousStatus">Statut du problème avant modification.</param>
    /// <param name="now">Horodatage de la modification.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>L'erreur à renvoyer, ou <c>null</c>.</returns>
    public async Task<DomainError?> ApplyAsync(Problem problem, ProblemStatus previousStatus, DateTimeOffset now, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (problem.Immobilized != true || previousStatus == problem.Status)
        {
            return null;
        }

        VehicleStatus? target = null;
        if (problem.Status == ProblemStatus.Resolved)
        {
            var otherOpen = await problems.HasOpenImmobilizingAsync(problem.VehicleId, problem.Id, cancellationToken);
            target = otherOpen ? null : VehicleStatus.Active;
        }
        else if (previousStatus == ProblemStatus.Resolved)
        {
            target = VehicleStatus.Immobilized;
        }

        if (target is not { } status)
        {
            return null;
        }

        var vehicle = await vehicles.GetByIdAsync(problem.VehicleId, cancellationToken);
        if (vehicle is null || vehicle.Status == VehicleStatus.Retired)
        {
            return null;
        }

        // Retour en service seulement depuis « immobilisé » ; la réouverture n'immobilise qu'un véhicule en service.
        var expected = status == VehicleStatus.Active ? VehicleStatus.Immobilized : VehicleStatus.Active;
        if (vehicle.Status != expected)
        {
            return null;
        }

        return vehicle.ChangeStatus(status, now).Error;
    }
}
