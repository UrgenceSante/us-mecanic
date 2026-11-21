import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { useId, type ReactNode } from "react";
import useBelow from "../hooks/useBelow";

interface ConfirmDialogProps {
  title: string;
  /** Verbe d'action, ex. « Supprimer » ou « Sortir du parc ». */
  confirmLabel: string;
  onConfirm: () => void;
  onClose: () => void;
  submitting?: boolean;
  error?: string | null;
  /** Ajoute « Cette action est définitive. » (vrai par défaut). */
  definitive?: boolean;
  /** Bouton principal en couleur d'alerte (vrai par défaut). */
  destructive?: boolean;
  children: ReactNode;
}

const buttonSx = { minHeight: 44 };

/** Confirmation d'une action destructrice : bouton principal rouge à droite, texte « Cette action est définitive ». */
export default function ConfirmDialog(props: ConfirmDialogProps) {
  const { title, confirmLabel, onConfirm, onClose, submitting = false, error, definitive = true, destructive = true } = props;
  const titleId = useId();
  const fullScreen = useBelow("sm");
  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs" fullScreen={fullScreen} aria-labelledby={titleId}>
      <DialogTitle id={titleId}>{title}</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" role="alert" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText component="div">{props.children}</DialogContentText>
        {definitive && <DialogContentText sx={{ marginTop: 1 }}>Cette action est définitive.</DialogContentText>}
      </DialogContent>
      <DialogActions sx={{ gap: 1 }}>
        <Button sx={buttonSx} onClick={onClose}>
          Annuler
        </Button>
        <Button
          variant="contained"
          color={destructive ? "error" : "primary"}
          disabled={submitting}
          sx={buttonSx}
          onClick={onConfirm}
        >
          {confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
