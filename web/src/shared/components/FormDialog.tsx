import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle } from "@mui/material";
import { useId, useRef, type FormEvent, type ReactNode } from "react";
import useBelow from "../hooks/useBelow";

interface FormDialogProps {
  title: string;
  /** Appelé à la soumission du formulaire (touche Entrée ou bouton principal) ; aucune validation native. */
  onSubmit: () => void;
  onClose: () => void;
  /** Verbe d'action, ex. « Enregistrer » ou « Ajouter ». */
  submitLabel: string;
  submitColor?: "primary" | "warning";
  submitting?: boolean;
  /** Erreur globale (API) affichée dans une alerte annoncée. */
  error?: string | null;
  /** Affiche seulement « Fermer » (ex. bilan après l'action). */
  done?: boolean;
  open?: boolean;
  maxWidth?: "xs" | "sm";
  children: ReactNode;
}

const firstFieldSelector =
  "input:not([type=hidden]):not([type=checkbox]):not([type=radio]):not([disabled]):not([readonly]):not([aria-hidden=true]), textarea:not([disabled]):not([readonly]), [role=combobox]:not([aria-disabled=true])";

const buttonSx = { minHeight: 44 };

/** Dialogue de formulaire : titre relié, `noValidate`, action principale à droite, plein écran sur mobile, focus sur le premier champ. */
export default function FormDialog(props: FormDialogProps) {
  const { title, onSubmit, onClose, submitLabel, submitColor = "primary", submitting = false, error, done = false } = props;
  const titleId = useId();
  const content = useRef<HTMLDivElement>(null);
  const fullScreen = useBelow("sm");

  const focusFirstField = () => {
    const root = content.current;
    if (!root || root.contains(document.activeElement)) return;
    root.querySelector<HTMLElement>(firstFieldSelector)?.focus();
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit();
  };

  return (
    <Dialog
      open={props.open ?? true}
      onClose={onClose}
      fullWidth
      maxWidth={props.maxWidth ?? "sm"}
      fullScreen={fullScreen}
      aria-labelledby={titleId}
      slotProps={{ transition: { onEntered: focusFirstField } }}
    >
      <form noValidate onSubmit={handleSubmit}>
        <DialogTitle id={titleId}>{title}</DialogTitle>
        <DialogContent ref={content}>
          {error && (
            <Alert severity="error" role="alert" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          {props.children}
        </DialogContent>
        <DialogActions sx={{ gap: 1 }}>
          {done ? (
            <Button variant="contained" sx={buttonSx} onClick={onClose}>
              Fermer
            </Button>
          ) : (
            <>
              <Button sx={buttonSx} onClick={onClose}>
                Annuler
              </Button>
              <Button type="submit" variant="contained" color={submitColor} disabled={submitting} sx={buttonSx}>
                {submitLabel}
              </Button>
            </>
          )}
        </DialogActions>
      </form>
    </Dialog>
  );
}
