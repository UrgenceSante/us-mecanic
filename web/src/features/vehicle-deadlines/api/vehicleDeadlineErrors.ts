import { mileageErrorCode, mileageErrorMessage } from "../../mileage/api/mileageErrors";

export const KILOMETERS_REQUIRED = "deadline_completion.kilometers_required";

const messages: Record<string, string> = {
  [KILOMETERS_REQUIRED]: "Cette échéance a un critère kilométrique : le kilométrage de réalisation est obligatoire.",
  "deadline_completion.date_in_future": "La date de réalisation ne peut pas être dans le futur.",
  "deadline_completion.comment_too_long": "Le commentaire est trop long.",
  "intervention.amount_negative": "Le montant ne peut pas être négatif.",
  "intervention.amount_too_precise": "Le montant est limité à 2 décimales.",
  "intervention.amount_too_large": "Le montant dépasse la valeur maximale autorisée.",
  "intervention.garage_unknown": "Ce garage n'existe plus.",
  "intervention.garage_inactive": "Ce garage est désactivé : choisissez-en un autre.",
  "vehicle_deadline.already_active": "Une échéance de ce type est déjà en cours pour ce véhicule.",
  "vehicle_deadline.closed": "Cette échéance est déjà close.",
  "vehicle_deadline.criterion_required": "Indiquez au moins une date ou un kilométrage d'échéance.",
  "vehicle_deadline.interval_km_invalid": "La périodicité en km doit être strictement positive.",
  "vehicle_deadline.interval_months_invalid": "La périodicité en mois doit être strictement positive.",
  "vehicle_deadline.kilometers_invalid": "Le kilométrage ne peut pas être négatif.",
  "vehicle_deadline.last_done_in_future": "La dernière réalisation ne peut pas être dans le futur.",
  "vehicle_deadline.not_found": "Cette échéance n'existe plus.",
  "vehicle_deadline.type_inactive": "Ce type d'échéance est désactivé.",
  "vehicle_deadline.type_unknown": "Ce type d'échéance n'existe plus.",
};

/** Message français d'une erreur d'échéance, par code métier ; repli sur les messages de relevé puis le titre. */
export function deadlineErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  if (code && messages[code]) return messages[code];
  return mileageErrorMessage(error);
}
