import { HIGH_MILEAGE_CONFIRMATION_REQUIRED, REASON_REQUIRED, mileageErrorCode } from "../../mileage/api/mileageErrors";
import { deadlineErrorMessage } from "./vehicleDeadlineErrors";

export interface CompletionFlow {
  reasonRequired: boolean;
  confirmationRequired: boolean;
  /** Message d'erreur à afficher (hors cas motif / confirmation, gérés par le formulaire). */
  message: string | null;
}

/** Interprète l'erreur d'une réalisation : motif requis, confirmation > 1 000 km/jour, ou message traduit. */
export function completionFlow(error: unknown): CompletionFlow {
  if (!error) return { reasonRequired: false, confirmationRequired: false, message: null };
  const code = mileageErrorCode(error);
  const reasonRequired = code === REASON_REQUIRED;
  const confirmationRequired = code === HIGH_MILEAGE_CONFIRMATION_REQUIRED;
  const message = reasonRequired || confirmationRequired ? null : deadlineErrorMessage(error);
  return { reasonRequired, confirmationRequired, message };
}
