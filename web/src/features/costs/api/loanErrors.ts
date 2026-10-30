import { problemMessage } from "../../../shared/api/problemMessage";
import { mileageErrorCode } from "../../mileage/api/mileageErrors";

const messages: Record<string, string> = {
  "loan.not_found": "Cet emprunt n'existe plus.",
  "loan.lender_required": "L'organisme prêteur est obligatoire.",
  "loan.lender_too_long": "Le nom de l'organisme est trop long.",
  "loan.borrowed_amount_required": "Le montant emprunté est obligatoire.",
  "loan.monthly_payment_required": "La mensualité est obligatoire.",
  "loan.borrowed_amount_negative": "Le montant emprunté ne peut pas être négatif.",
  "loan.monthly_payment_negative": "La mensualité ne peut pas être négative.",
  "loan.amount_precision": "Les montants sont limités à 2 décimales.",
  "loan.amount_too_large": "Un montant dépasse la valeur maximale autorisée.",
  "loan.start_date_required": "La date de début est obligatoire.",
  "loan.duration_invalid": "La durée doit être un nombre entier de mois strictement positif.",
  "loan.duration_too_long": "La durée dépasse la valeur maximale autorisée.",
  "vehicle.retired": "Ce véhicule est sorti du parc : aucun emprunt ne peut être ajouté.",
  "vehicle.not_found": "Ce véhicule n'existe plus.",
};

/** Message français d'une erreur d'emprunt, selon le code métier (repli sur le titre ProblemDetails). */
export function loanErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  return (code && messages[code]) || problemMessage(error);
}
