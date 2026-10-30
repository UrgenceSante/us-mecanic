import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";

const messages: Record<string, string> = {
  "expense_category.name_required": "Le nom de la catégorie est obligatoire.",
  "expense_category.name_too_long": "Le nom de la catégorie est trop long.",
  "expense_category.name_already_exists": "Une catégorie portant ce nom existe déjà.",
  "expense_category.not_found": "Cette catégorie n'existe plus.",
  "expense_category.inactive": "Cette catégorie est désactivée.",
};

/** Message français d'une erreur liée aux catégories de dépense (repli sur le titre ProblemDetails). */
export function expenseCategoryErrorMessage(error: unknown): string {
  const code = isAxiosError<ProblemDetails>(error) ? error.response?.data?.code : undefined;
  if (code && messages[code]) return messages[code];
  return problemMessage(error);
}
