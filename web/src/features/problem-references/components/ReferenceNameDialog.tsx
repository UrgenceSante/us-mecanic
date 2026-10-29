import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";

interface ReferenceNameDialogProps {
  /** Libellé singulier du référentiel, ex. « nature » ou « type d'action ». */
  singular: string;
  feminine: boolean;
  /** Libellé avec article défini, ex. « l'élément » ou « la nature ». */
  definite: string;
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  error: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export default function ReferenceNameDialog(props: ReferenceNameDialogProps) {
  const { singular, feminine, definite, initialName, submitting, error, onSubmit, onClose } = props;
  const [name, setName] = useState(initialName ?? "");

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit(name.trim());
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{initialName === undefined ? `Ajouter ${feminine ? "une" : "un"} ${singular}` : `Renommer ${definite}`}</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            label="Nom"
            required
            autoFocus
            fullWidth
            margin="dense"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={submitting}>
            Enregistrer
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
