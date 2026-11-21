import { Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";

interface Props {
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  /** Erreur de l'API (ex. nom en double), affichée sous le champ. */
  error: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export default function DocumentCategoryNameDialog({ initialName, submitting, error, onSubmit, onClose }: Props) {
  const [name, setName] = useState(initialName ?? "");

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit(name.trim());
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{initialName === undefined ? "Ajouter une catégorie" : "Renommer la catégorie"}</DialogTitle>
        <DialogContent>
          <TextField
            label="Nom"
            required
            autoFocus
            fullWidth
            margin="dense"
            value={name}
            onChange={(e) => setName(e.target.value)}
            error={error !== null}
            helperText={error ?? " "}
            slotProps={{ htmlInput: { maxLength: 100 } }}
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
