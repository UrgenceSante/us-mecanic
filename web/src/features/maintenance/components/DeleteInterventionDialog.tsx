import { Alert } from "@mui/material";
import ConfirmDialog from "../../../shared/components/ConfirmDialog";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { Intervention } from "../types/intervention";

interface DeleteInterventionDialogProps {
  intervention: Intervention;
  submitting: boolean;
  error: string | null;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeleteInterventionDialog({
  intervention,
  submitting,
  error,
  onConfirm,
  onClose,
}: DeleteInterventionDialogProps) {
  return (
    <ConfirmDialog
      title="Supprimer l'intervention"
      confirmLabel="Supprimer"
      submitting={submitting}
      error={error}
      onConfirm={onConfirm}
      onClose={onClose}
    >
      Supprimer l'intervention du {formatDateOnly(intervention.date)} ({intervention.work}) ?
      <Alert severity="warning" sx={{ marginTop: 2 }}>
        Les échéances réalisées par cette intervention ne seront pas recalculées et le relevé kilométrique est conservé.
      </Alert>
    </ConfirmDialog>
  );
}
