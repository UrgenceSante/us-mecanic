import ConfirmDialog from "../../../shared/components/ConfirmDialog";
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
    <ConfirmDialog
      title="Supprimer la dépense"
      confirmLabel="Supprimer"
      submitting={submitting}
      error={error}
      onConfirm={onConfirm}
      onClose={onClose}
    >
      Supprimer la dépense de {formatEuro(expense.amount)} du {formatDateOnly(expense.date)} ?
    </ConfirmDialog>
  );
}
