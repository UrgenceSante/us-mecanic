import { problemMessage } from "../../../shared/api/problemMessage";
import { mileageErrorCode } from "../../mileage/api/mileageErrors";

const messages: Record<string, string> = {
  "expense.not_found": "Cette dépense n'existe plus.",
  "expense.not_manual": "Cette dépense n'est pas une saisie manuelle : elle ne peut être ni modifiée ni supprimée.",
  "expense.family_not_manual": "Seules les dépenses ponctuelles peuvent être saisies à la main.",
  "expense.date_required": "La date est obligatoire.",
  "expense.category_required": "La catégorie est obligatoire.",
  "expense.amount_required": "Le montant est obligatoire.",
  "expense.amount_negative": "Le montant ne peut pas être négatif.",
  "expense.amount_too_precise": "Le montant est limité à 2 décimales.",
  "expense.amount_too_large": "Le montant dépasse la valeur maximale autorisée.",
  "expense.label_too_long": "Le libellé est trop long.",
  "expense_category.unknown": "Cette catégorie n'existe pas.",
  "expense_category.not_found": "Cette catégorie n'existe pas.",
  "expense_category.inactive": "Cette catégorie est désactivée : choisissez une catégorie active.",
  "vehicle.retired": "Ce véhicule est sorti du parc : modification impossible.",
  "vehicle.not_found": "Ce véhicule n'existe plus.",
};

/** Message français d'une erreur de dépense, selon le code métier (repli sur le titre ProblemDetails). */
export function expenseErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  return (code && messages[code]) || problemMessage(error);
}
