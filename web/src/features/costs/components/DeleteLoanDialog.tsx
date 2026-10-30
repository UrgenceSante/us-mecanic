import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { Loan } from "../types/loan";

interface DeleteLoanDialogProps {
  loan: Loan;
  submitting: boolean;
  error: string | null;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeleteLoanDialog({ loan, submitting, error, onConfirm, onClose }: DeleteLoanDialogProps) {
  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Supprimer l'emprunt</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          Supprimer l'emprunt de {formatEuro(loan.borrowedAmount)} auprès de {loan.lender} ?
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
