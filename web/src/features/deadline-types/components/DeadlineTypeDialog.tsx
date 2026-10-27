import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";
import type { DeadlineType, DeadlineTypeInput } from "../types/deadlineType";

interface DeadlineTypeDialogProps {
  /** Type modifié ; absent en création. */
  type?: DeadlineType;
  submitting: boolean;
  error: string | null;
  onSubmit: (input: DeadlineTypeInput) => void;
  onClose: () => void;
}

const toText = (value: number | null | undefined) => (value == null ? "" : String(value));
const toNumber = (text: string) => (text.trim() === "" ? null : Number(text));

export default function DeadlineTypeDialog({ type, submitting, error, onSubmit, onClose }: DeadlineTypeDialogProps) {
  const [name, setName] = useState(type?.name ?? "");
  const [months, setMonths] = useState(toText(type?.defaultIntervalMonths));
  const [km, setKm] = useState(toText(type?.defaultIntervalKm));
  const [soonDays, setSoonDays] = useState(toText(type?.dueSoonDays));
  const [soonKm, setSoonKm] = useState(toText(type?.dueSoonKm));

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit({
      name: name.trim(),
      defaultIntervalMonths: toNumber(months),
      defaultIntervalKm: toNumber(km),
      dueSoonDays: toNumber(soonDays),
      dueSoonKm: toNumber(soonKm),
    });
  };

  const numberField = (label: string, value: string, set: (v: string) => void, helper: string) => (
    <TextField
      label={label}
      type="number"
      fullWidth
      margin="dense"
      value={value}
      helperText={helper}
      slotProps={{ htmlInput: { min: 1 } }}
      onChange={(e) => set(e.target.value)}
    />
  );

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{type ? "Modifier le type d'échéance" : "Ajouter un type d'échéance"}</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            label="Nom"
            placeholder="Agrément ARS, extincteur…"
            required
            autoFocus
            fullWidth
            margin="dense"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
          <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
            {numberField("Périodicité (mois)", months, setMonths, "Vide : pas de périodicité en mois")}
            {numberField("Périodicité (km)", km, setKm, "Vide : pas de périodicité en km")}
          </Stack>
          <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
            {numberField("Seuil « proche » (jours)", soonDays, setSoonDays, "Vide : seuil global")}
            {numberField("Seuil « proche » (km)", soonKm, setSoonKm, "Vide : seuil global")}
          </Stack>
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
