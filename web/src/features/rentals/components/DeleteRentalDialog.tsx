import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
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
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Supprimer la location</DialogTitle>
      <DialogContent>
        {error ? (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {rentalErrorMessage(error)}
          </Alert>
        ) : null}
        <DialogContentText>
          Supprimer la location à {rental.tenant.name} {rentalPeriod(rental)} ? Cette action est définitive.
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
