import ConfirmDialog from "../../../shared/components/ConfirmDialog";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { MileageReading } from "../types/mileageReading";

interface DeleteReadingDialogProps {
  reading: MileageReading;
  submitting: boolean;
  error: string | null;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeleteReadingDialog({ reading, submitting, error, onConfirm, onClose }: DeleteReadingDialogProps) {
  return (
    <ConfirmDialog
      title="Supprimer le relevé"
      confirmLabel="Supprimer"
      submitting={submitting}
      error={error}
      onConfirm={onConfirm}
      onClose={onClose}
    >
      Supprimer le relevé de {reading.kilometers.toLocaleString("fr-FR")} km du {formatDateOnly(reading.date)} ? Le km
      actuel du véhicule sera recalculé.
    </ConfirmDialog>
  );
}
