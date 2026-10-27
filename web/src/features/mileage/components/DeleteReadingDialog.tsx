import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
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
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Supprimer le relevé</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          Supprimer le relevé de {reading.kilometers.toLocaleString("fr-FR")} km du {formatDateOnly(reading.date)} ?
          Le km actuel du véhicule sera recalculé.
        </DialogContentText>
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
