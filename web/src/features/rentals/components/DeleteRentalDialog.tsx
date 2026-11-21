import ConfirmDialog from "../../../shared/components/ConfirmDialog";
import { rentalErrorMessage } from "../api/rentalErrors";
import type { Rental } from "../types/rental";
import { rentalPeriod } from "./rentalFormat";

interface DeleteRentalDialogProps {
  rental: Rental;
  submitting: boolean;
  error: unknown;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeleteRentalDialog({ rental, submitting, error, onConfirm, onClose }: DeleteRentalDialogProps) {
  return (
    <ConfirmDialog
      title="Supprimer la location"
      confirmLabel="Supprimer"
      submitting={submitting}
      error={error ? rentalErrorMessage(error) : null}
      onConfirm={onConfirm}
      onClose={onClose}
    >
      Supprimer la location à {rental.tenant.name} {rentalPeriod(rental)} ?
    </ConfirmDialog>
  );
}
