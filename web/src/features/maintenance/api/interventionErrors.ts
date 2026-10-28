import { mileageErrorCode } from "../../mileage/api/mileageErrors";
import { completionFlow, type CompletionFlow } from "../../vehicle-deadlines/api/completionFlow";
import { deadlineErrorMessage } from "../../vehicle-deadlines/api/vehicleDeadlineErrors";

const messages: Record<string, string> = {
  "intervention.not_found": "Cette intervention n'existe plus.",
  "intervention.work_required": "Les travaux réalisés sont obligatoires.",
  "intervention.work_too_long": "Les travaux sont trop longs.",
  "intervention.date_in_future": "La date de l'intervention ne peut pas être dans le futur.",
  "intervention.deadline_duplicated": "Une échéance ne peut être cochée qu'une seule fois.",
  "intervention.deadline_unknown": "Une des échéances cochées n'existe plus pour ce véhicule.",
  "intervention.deadline_closed": "Une des échéances cochées est déjà close.",
  "intervention.garage_unknown": "Ce garage n'existe plus.",
  "intervention.garage_inactive": "Ce garage est désactivé : choisissez-en un autre.",
};

/** Message français d'une erreur d'intervention (repli sur les erreurs d'échéance et de relevé). */
export function interventionErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  if (code && messages[code]) return messages[code];
  return deadlineErrorMessage(error);
}

/** Même flux que la réalisation d'une échéance : motif requis, confirmation > 1 000 km/jour, sinon message. */
export function interventionFlow(error: unknown): CompletionFlow {
  const flow = completionFlow(error);
  return flow.message === null ? flow : { ...flow, message: interventionErrorMessage(error) };
}
