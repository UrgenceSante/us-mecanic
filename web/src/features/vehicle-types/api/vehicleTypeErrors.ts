import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";

const messages: Record<string, string> = {
  "vehicle_type.name_required": "Le nom du type est obligatoire.",
  "vehicle_type.name_too_long": "Le nom du type est trop long.",
  "vehicle_type.name_already_exists": "Un type portant ce nom existe déjà.",
  "vehicle_type.not_found": "Ce type de véhicule n'existe plus.",
  "vehicle_type.unknown": "Ce type de véhicule est inconnu.",
  "vehicle_type.inactive": "Ce type de véhicule est désactivé.",
};

/** Message français d'une erreur liée aux types, selon le code métier (repli sur le titre ProblemDetails). */
export function vehicleTypeErrorMessage(error: unknown): string {
  if (isAxiosError<ProblemDetails>(error)) {
    const code = error.response?.data?.code;
    if (code && messages[code]) return messages[code];
  }
  return problemMessage(error);
}
