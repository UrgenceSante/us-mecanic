import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";

export const REASON_REQUIRED = "mileage_reading.reason_required";
export const HIGH_MILEAGE_CONFIRMATION_REQUIRED = "mileage_reading.high_mileage_confirmation_required";

const messages: Record<string, string> = {
  [REASON_REQUIRED]:
    "Ce kilométrage est incohérent avec les relevés voisins (inférieur au précédent ou supérieur au suivant) : indiquez un motif.",
  "mileage_reading.kilometers_negative": "Le kilométrage ne peut pas être négatif.",
  "mileage_reading.date_in_future": "La date du relevé ne peut pas être dans le futur.",
  "mileage_reading.reason_too_long": "Le motif est trop long.",
  "mileage_reading.not_found": "Ce relevé n'existe plus.",
  "vehicle.retired": "Ce véhicule est sorti du parc : aucun relevé ne peut être saisi.",
  "vehicle.not_found": "Ce véhicule n'existe plus.",
};

export function mileageErrorCode(error: unknown): string | undefined {
  return isAxiosError<ProblemDetails>(error) ? error.response?.data?.code : undefined;
}

/** Message français d'une erreur de relevé, selon le code métier (repli sur le titre ProblemDetails). */
export function mileageErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  if (code && messages[code]) return messages[code];
  return problemMessage(error);
}
