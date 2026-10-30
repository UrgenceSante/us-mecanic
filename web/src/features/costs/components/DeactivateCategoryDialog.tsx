import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

interface DeactivateCategoryDialogProps {
  name: string;
  submitting: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

export default function DeactivateCategoryDialog({ name, submitting, onConfirm, onClose }: DeactivateCategoryDialogProps) {
  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Désactiver la catégorie</DialogTitle>
      <DialogContent>
        <DialogContentText>
          Désactiver la catégorie « {name} » ? Elle ne sera plus proposée à la saisie mais restera affichée sur les
          dépenses existantes.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button variant="contained" color="warning" disabled={submitting} onClick={onConfirm}>
          Confirmer la désactivation
        </Button>
      </DialogActions>
    </Dialog>
  );
}
