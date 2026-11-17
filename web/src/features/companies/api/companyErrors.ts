import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";

const messages: Record<string, string> = {
  "company.name_required": "Le nom de la société est obligatoire.",
  "company.name_too_long": "Le nom de la société est trop long.",
  "company.name_already_exists": "Une société portant ce nom existe déjà.",
  "company.not_found": "Cette société n'existe plus.",
};

/** Code métier d'une erreur ProblemDetails, s'il y en a un. */
export function companyErrorCode(error: unknown): string | undefined {
  return isAxiosError<ProblemDetails>(error) ? error.response?.data?.code : undefined;
}

/** Message français d'une erreur liée aux sociétés, selon le code métier (repli sur le titre ProblemDetails). */
export function companyErrorMessage(error: unknown): string {
  const code = companyErrorCode(error);
  if (code && messages[code]) return messages[code];
  return problemMessage(error);
}
