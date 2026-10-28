import { isAxiosError } from "axios";
import { problemMessage, type ProblemDetails } from "../../../shared/api/problemMessage";

const messages: Record<string, string> = {
  "deadline_type.name_required": "Le nom du type d'échéance est obligatoire.",
  "deadline_type.name_too_long": "Le nom du type d'échéance est trop long.",
  "deadline_type.name_already_exists": "Un type d'échéance portant ce nom existe déjà.",
  "deadline_type.not_found": "Ce type d'échéance n'existe plus.",
  "deadline_type.interval_months_invalid": "La périodicité en mois doit être supérieure à 0.",
  "deadline_type.interval_km_invalid": "La périodicité en kilomètres doit être supérieure à 0.",
  "deadline_type.due_soon_days_invalid": "Le seuil « proche » en jours doit être supérieur à 0.",
  "deadline_type.due_soon_km_invalid": "Le seuil « proche » en kilomètres doit être supérieur à 0.",
  "vehicle_deadline.type_inactive": "Ce type d'échéance est désactivé : il ne peut plus être ajouté à des véhicules.",
  "vehicle_deadline.one_off_not_assignable":
    "Un type ponctuel (sans périodicité) ne peut pas être ajouté en lot : sa date dépend de chaque véhicule.",
  "vehicle_deadline.vehicle_types_required": "Choisissez au moins un type de véhicule.",
  "vehicle_deadline.last_done_in_future": "La date de dernière réalisation ne peut pas être dans le futur.",
  "vehicle_type.unknown": "Un des types de véhicule n'existe plus.",
  "deadline_thresholds.days_invalid": "Le seuil global en jours doit être supérieur à 0.",
  "deadline_thresholds.km_invalid": "Le seuil global en kilomètres doit être supérieur à 0.",
};

/** Message français d'une erreur liée aux échéances, selon le code métier (repli sur le titre ProblemDetails). */
export function deadlineTypeErrorMessage(error: unknown): string {
  if (isAxiosError<ProblemDetails>(error)) {
    const code = error.response?.data?.code;
    if (code && messages[code]) return messages[code];
  }
  return problemMessage(error);
}
