import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

interface CloseActionDialogProps {
  submitting: boolean;
  error: string | null;
  onConfirm: () => void;
  onClose: () => void;
}

export default function CloseActionDialog({ submitting, error, onConfirm, onClose }: CloseActionDialogProps) {
  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Clôturer l'action</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          Clôturer cette action ? Elle ne pourra plus être modifiée ; si c'est la dernière, le problème sera résolu.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button variant="contained" disabled={submitting} onClick={onConfirm}>
          Clôturer
        </Button>
      </DialogActions>
    </Dialog>
  );
}
