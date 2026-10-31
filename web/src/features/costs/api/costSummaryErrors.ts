import { problemMessage } from "../../../shared/api/problemMessage";
import { mileageErrorCode } from "../../mileage/api/mileageErrors";

const messages: Record<string, string> = {
  "cost_summary.from_required": "La date de début de la période est obligatoire.",
  "cost_summary.to_required": "La date de fin de la période est obligatoire.",
  "cost_summary.invalid_range": "La date de début doit précéder ou égaler la date de fin.",
  "cost_summary.range_too_long": "La période demandée est trop longue.",
  "cost_summary.invalid_granularity": "La granularité doit être le mois ou l'année.",
  "vehicle.not_found": "Ce véhicule n'existe plus.",
};

/** Message français d'une erreur de synthèse, selon le code métier (repli sur le titre ProblemDetails). */
export function costSummaryErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  return (code && messages[code]) || problemMessage(error);
}
