using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Corps d'une requête d'ajout d'échéance. Les périodicités sont prises telles quelles (préremplies par le front depuis le type).</summary>
/// <param name="DeadlineTypeId">Type d'échéance, actif.</param>
/// <param name="IntervalMonths">Périodicité en mois ; vide avec <paramref name="IntervalKm"/> vide = échéance ponctuelle.</param>
/// <param name="IntervalKm">Périodicité en kilomètres.</param>
/// <param name="LastDoneDate">Date de dernière réalisation (périodique).</param>
/// <param name="LastDoneKm">Km de dernière réalisation (périodique).</param>
/// <param name="DueDate">Date d'échéance saisie directement (ponctuelle uniquement).</param>
/// <param name="DueKm">Km d'échéance saisi directement (ponctuelle uniquement).</param>
public sealed record VehicleDeadlineInput(
    Guid DeadlineTypeId,
    int? IntervalMonths = null,
    int? IntervalKm = null,
    DateOnly? LastDoneDate = null,
    int? LastDoneKm = null,
    DateOnly? DueDate = null,
    int? DueKm = null)
{
    public VehicleDeadlineSchedule ToSchedule() => new(IntervalMonths, IntervalKm, LastDoneDate, LastDoneKm, DueDate, DueKm);
}
