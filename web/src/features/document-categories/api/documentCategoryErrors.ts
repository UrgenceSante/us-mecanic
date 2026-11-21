import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";

const messages: Record<string, string> = {
  "document_category.name_required": "Le nom de la catégorie est obligatoire.",
  "document_category.name_too_long": "Le nom de la catégorie est limité à 100 caractères.",
  "document_category.name_already_exists": "Une catégorie porte déjà ce nom.",
  "document_category.not_found": "Cette catégorie n'existe plus.",
};

/** Code métier d'une erreur ProblemDetails, s'il y en a un. */
export function documentCategoryErrorCode(error: unknown): string | undefined {
  return isAxiosError<ProblemDetails>(error) ? error.response?.data?.code : undefined;
}

/** Message français d'une erreur liée aux catégories de document (repli sur le titre ProblemDetails). */
export function documentCategoryErrorMessage(error: unknown): string {
  const code = documentCategoryErrorCode(error);
  if (code && messages[code]) return messages[code];
  return problemMessage(error);
}
