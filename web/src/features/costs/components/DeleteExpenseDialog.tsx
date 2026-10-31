import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { Expense } from "../types/expense";

interface DeleteExpenseDialogProps {
  expense: Expense;
  submitting: boolean;
  error: string | null;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeleteExpenseDialog({ expense, submitting, error, onConfirm, onClose }: DeleteExpenseDialogProps) {
  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Supprimer la dépense</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          Supprimer la dépense de {formatEuro(expense.amount)} du {formatDateOnly(expense.date)} ?
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
