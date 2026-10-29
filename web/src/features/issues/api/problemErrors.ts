import { problemMessage } from "../../../shared/api/problemMessage";
import { mileageErrorCode } from "../../mileage/api/mileageErrors";

const messages: Record<string, string> = {
  "problem.not_found": "Ce problème n'existe plus.",
  "problem.already_analyzed": "Ce problème a déjà été analysé : l'analyse n'est pas modifiable.",
  "problem.already_resolved": "Ce problème est déjà résolu.",
  "problem.not_analyzed": "Le problème doit être analysé avant d'ajouter une action.",
  "problem.analysis_required": "Le texte de l'analyse est obligatoire.",
  "problem.analysis_too_long": "Le texte de l'analyse est trop long.",
  "problem.nature_unknown": "Cette nature n'existe pas.",
  "problem.element_unknown": "Cet élément n'existe pas.",
  "problem.vehicle_unknown": "Ce véhicule n'existe plus.",
  "problem_action.not_found": "Cette action n'existe plus.",
  "problem_action.already_closed": "Cette action est déjà clôturée.",
  "problem_action.comment_too_long": "Le commentaire est trop long.",
  "problem_action.actor_unknown": "Cet acteur n'existe pas.",
  "problem_action.type_unknown": "Ce type d'action n'existe pas.",
  "problem_action.constraint_unknown": "Cette contrainte n'existe pas.",
  "problem_action.intervention_invalid": "Cette intervention n'appartient pas à ce véhicule.",
  "problem_nature.inactive": "Cette nature est désactivée : choisissez-en une autre.",
  "problem_element.inactive": "Cet élément est désactivé : choisissez-en un autre.",
  "problem_actor.inactive": "Cet acteur est désactivé : choisissez-en un autre.",
  "problem_action_type.inactive": "Ce type d'action est désactivé : choisissez-en un autre.",
  "problem_constraint.inactive": "Cette contrainte est désactivée : choisissez-en une autre.",
  "vehicle.retired": "Ce véhicule est sorti du parc : le problème ne peut plus être modifié.",
};

/** Message français d'une erreur d'analyse ou d'action, selon le code métier (repli sur le titre ProblemDetails). */
export function problemErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  if (code && messages[code]) return messages[code];
  return problemMessage(error);
}
