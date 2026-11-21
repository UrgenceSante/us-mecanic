import ConfirmDialog from "../../../shared/components/ConfirmDialog";

interface CloseActionDialogProps {
  submitting: boolean;
  error: string | null;
  onConfirm: () => void;
  onClose: () => void;
}

export default function CloseActionDialog({ submitting, error, onConfirm, onClose }: CloseActionDialogProps) {
  return (
    <ConfirmDialog
      title="Clôturer l'action"
      confirmLabel="Clôturer"
      destructive={false}
      submitting={submitting}
      error={error}
      onConfirm={onConfirm}
      onClose={onClose}
    >
      Clôturer cette action ? Elle ne pourra plus être modifiée ; si c'est la dernière, le problème sera résolu.
    </ConfirmDialog>
  );
}
