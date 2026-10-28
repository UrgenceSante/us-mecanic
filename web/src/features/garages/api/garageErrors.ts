import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";

const messages: Record<string, string> = {
  "garage.name_required": "Le nom du garage est obligatoire.",
  "garage.name_too_long": "Le nom du garage est trop long.",
  "garage.name_already_exists": "Un garage portant ce nom existe déjà.",
  "garage.not_found": "Ce garage n'existe plus.",
};

/** Code métier d'une erreur ProblemDetails, s'il y en a un. */
export function garageErrorCode(error: unknown): string | undefined {
  return isAxiosError<ProblemDetails>(error) ? error.response?.data?.code : undefined;
}

/** Message français d'une erreur liée aux garages, selon le code métier (repli sur le titre ProblemDetails). */
export function garageErrorMessage(error: unknown): string {
  const code = garageErrorCode(error);
  if (code && messages[code]) return messages[code];
  return problemMessage(error);
}
