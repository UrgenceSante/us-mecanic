import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { documentDisplayName, type VehicleDocument } from "../types/document";

interface DeleteDocumentDialogProps {
  document: VehicleDocument;
  submitting: boolean;
  error: string | null;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeleteDocumentDialog({ document, submitting, error, onConfirm, onClose }: DeleteDocumentDialogProps) {
  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Supprimer le document</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          Supprimer « {documentDisplayName(document)} » ? Cette action est définitive.
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
