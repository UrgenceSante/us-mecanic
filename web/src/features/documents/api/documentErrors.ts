import { problemMessage } from "../../../shared/api/problemMessage";
import { mileageErrorCode } from "../../mileage/api/mileageErrors";

const messages: Record<string, string> = {
  "document.not_found": "Ce document n'existe plus.",
  "document.category_required": "La catégorie est obligatoire.",
  "document.label_too_long": "Le libellé est limité à 200 caractères.",
  "document.file_required": "Le fichier est obligatoire.",
  "document.file_empty": "Le fichier est vide.",
  "document.file_too_large": "Fichier trop volumineux (10 Mo maximum).",
  "document.file_type_unsupported": "Format non accepté : PDF, JPEG ou PNG uniquement.",
  "document_category.inactive": "Cette catégorie est désactivée : choisissez une catégorie active.",
  "document_category.not_found": "Cette catégorie n'existe pas.",
  "vehicle.not_found": "Ce véhicule n'existe plus.",
};

/** Message français d'une erreur de document, selon le code métier (repli sur le titre ProblemDetails). */
export function documentErrorMessage(error: unknown): string {
  const code = mileageErrorCode(error);
  return (code && messages[code]) || problemMessage(error);
}
