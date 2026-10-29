import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";
import type { ProblemReferenceConfig } from "../types/problemReference";

/** Message français d'une erreur d'un référentiel de problèmes, selon le code métier (repli sur le titre ProblemDetails). */
export function problemReferenceErrorMessage(error: unknown, config: ProblemReferenceConfig): string {
  const code = isAxiosError<ProblemDetails>(error) ? error.response?.data?.code : undefined;
  const { errorPrefix: p, definite, singular, feminine } = config;
  const messages: Record<string, string> = {
    [`${p}.name_required`]: `Le nom de ${definite} est obligatoire.`,
    [`${p}.name_too_long`]: `Le nom de ${definite} est trop long.`,
    [`${p}.name_already_exists`]: `${feminine ? "Une" : "Un"} ${singular} portant ce nom existe déjà.`,
    [`${p}.not_found`]: `${feminine ? "Cette" : "Ce"} ${singular} n'existe plus.`,
    [`${p}.inactive`]: `${feminine ? "Cette" : "Ce"} ${singular} est désactivé${feminine ? "e" : ""}.`,
  };
  if (code && messages[code]) return messages[code];
  return problemMessage(error);
}
