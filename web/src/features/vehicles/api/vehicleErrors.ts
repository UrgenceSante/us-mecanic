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

export interface OwnerFieldFeedback {
  /** Texte d'aide ou d'erreur du champ propriétaire. */
  helperText: string | undefined;
  hasError: boolean;
  /** Message du bandeau pour les erreurs étrangères au champ propriétaire. */
  banner: string | null;
}

const CHOOSE_OWNER = "Choisissez la société propriétaire pour enregistrer";

/** Répartit une erreur d'enregistrement entre le champ propriétaire et le bandeau. */
export function ownerFieldFeedback(error: unknown, ownerOnly: boolean, ownerMissing: boolean): OwnerFieldFeedback {
  const ownerError = vehicleOwnerErrorMessage(error);
  // Propriétaire seul : « sorti du parc » est contradictoire, on invite à choisir la société.
  const retired = ownerOnly && companyErrorCode(error) === "vehicle.retired";
  const hasError = ownerError !== null || retired;
  return {
    helperText: ownerError ?? (retired || ownerMissing ? CHOOSE_OWNER : undefined),
    hasError,
    banner: error && !hasError ? vehicleErrorMessage(error) : null,
  };
}
