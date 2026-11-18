import { companyErrorCode } from "../../companies/api/companyErrors";
import { problemMessage } from "../../../shared/api/problemMessage";

/** Erreurs affichées à côté du champ « Société propriétaire ». */
const ownerMessages: Record<string, string> = {
  "vehicle.owner_cannot_be_removed": "Le propriétaire ne peut pas être retiré : choisissez une autre société.",
  "company.unknown": "Cette société n'existe plus : choisissez-en une autre.",
  "company.inactive": "Cette société est désactivée : choisissez-en une autre.",
};

const messages: Record<string, string> = {
  "vehicle.retired": "Le véhicule est sorti du parc : il ne peut plus être modifié.",
};

/** Message du champ propriétaire pour une erreur d'enregistrement, null si l'erreur concerne autre chose. */
export function vehicleOwnerErrorMessage(error: unknown): string | null {
  const code = companyErrorCode(error);
  return code && ownerMessages[code] ? ownerMessages[code] : null;
}

/** Message français d'une erreur d'enregistrement de véhicule (repli sur le titre ProblemDetails). */
export function vehicleErrorMessage(error: unknown): string {
  const code = companyErrorCode(error);
  return (code && messages[code]) || problemMessage(error);
}
