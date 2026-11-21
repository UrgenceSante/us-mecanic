import ConfirmDialog from "../../../shared/components/ConfirmDialog";
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
    <ConfirmDialog
      title="Supprimer l'emprunt"
      confirmLabel="Supprimer"
      submitting={submitting}
      error={error}
      onConfirm={onConfirm}
      onClose={onClose}
    >
      Supprimer l'emprunt de {formatEuro(loan.borrowedAmount)} auprès de {loan.lender} ?
    </ConfirmDialog>
  );
}
