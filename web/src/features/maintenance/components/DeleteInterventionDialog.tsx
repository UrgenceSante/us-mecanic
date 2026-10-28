import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
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
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Supprimer l'intervention</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          Supprimer l'intervention du {formatDateOnly(intervention.date)} ({intervention.work}) ?
        </DialogContentText>
        <Alert severity="warning" sx={{ marginTop: 2 }}>
          Les échéances réalisées par cette intervention ne seront pas recalculées et le relevé kilométrique est conservé.
        </Alert>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button color="error" variant="contained" disabled={submitting} onClick={onConfirm}>
          Supprimer
        </Button>
      </DialogActions>
    </Dialog>
  );
}
