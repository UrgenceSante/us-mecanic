import { companyErrorCode } from "../../companies/api/companyErrors";
import { problemMessage } from "../../../shared/api/problemMessage";

/** Champ du formulaire auquel une erreur de location se rapporte ; « general » : bandeau. */
export type RentalErrorField = "tenant" | "startDate" | "endDate" | "general";

/** Texte vide : on reprend le titre de l'API (le chevauchement nomme la location en conflit). */
const messages: Record<string, { field: RentalErrorField; text: string }> = {
  "rental.tenant_required": { field: "tenant", text: "La société locataire est obligatoire." },
  "rental.tenant_is_owner": { field: "tenant", text: "Le locataire ne peut pas être le propriétaire." },
  "company.unknown": { field: "tenant", text: "Cette société n'existe plus : choisissez-en une autre." },
  "company.inactive": { field: "tenant", text: "Cette société est désactivée : choisissez-en une autre." },
  "rental.start_date_required": { field: "startDate", text: "La date de début est obligatoire." },
  "rental.overlap": { field: "startDate", text: "" },
  "rental.end_before_start": {
    field: "endDate",
    text: "La date de fin doit être postérieure ou égale à la date de début.",
  },
  "rental.already_ended": { field: "endDate", text: "Cette location a déjà une date de fin : modifiez-la plutôt." },
  "rental.owner_required": { field: "general", text: "Renseignez d'abord le propriétaire du véhicule." },
  "rental.vehicle_retired": { field: "general", text: "Véhicule sorti du parc : aucune nouvelle location." },
  "rental.not_found": { field: "general", text: "Cette location n'existe plus." },
  "vehicle.not_found": { field: "general", text: "Ce véhicule n'existe plus." },
};

export interface RentalFieldError {
  field: RentalErrorField;
  message: string;
}

/** Erreur d'une location en français, avec le champ concerné ; null sans erreur. */
export function rentalFieldError(error: unknown): RentalFieldError | null {
  if (!error) return null;
  const code = companyErrorCode(error);
  const known = code ? messages[code] : undefined;
  if (!known) return { field: "general", message: problemMessage(error) };
  return { field: known.field, message: known.text || problemMessage(error) };
}

/** Message seul, pour un bandeau ou une notification. */
export function rentalErrorMessage(error: unknown): string {
  return rentalFieldError(error)?.message ?? problemMessage(error);
}
