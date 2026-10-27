import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  TextField,
} from "@mui/material";
import { useState, type FormEvent } from "react";
import type { DeadlineType } from "../../deadline-types/types/deadlineType";
import { todayLocalIso } from "../../mileage/types/date";
import type { VehicleDeadlineInput } from "../types/vehicleDeadline";

interface AddDeadlineDialogProps {
  types: DeadlineType[];
  submitting: boolean;
  error: string | null;
  onSubmit: (input: VehicleDeadlineInput) => void;
  onClose: () => void;
}

const toInt = (value: string): number | null => (value.trim() === "" ? null : Number(value));
const toStr = (value: number | null): string => (value === null ? "" : String(value));
const toDate = (value: string): string | null => (value === "" ? null : value);

export default function AddDeadlineDialog({ types, submitting, error, onSubmit, onClose }: AddDeadlineDialogProps) {
  const [typeId, setTypeId] = useState("");
  const [months, setMonths] = useState("");
  const [km, setKm] = useState("");
  const [lastDate, setLastDate] = useState("");
  const [lastKm, setLastKm] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [dueKm, setDueKm] = useState("");

  // Périodicité vide des deux côtés = échéance ponctuelle.
  const oneOff = months.trim() === "" && km.trim() === "";

  const chooseType = (id: string) => {
    setTypeId(id);
    const type = types.find((t) => t.id === id);
    setMonths(toStr(type?.defaultIntervalMonths ?? null));
    setKm(toStr(type?.defaultIntervalKm ?? null));
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit({
      deadlineTypeId: typeId,
      intervalMonths: toInt(months),
      intervalKm: toInt(km),
      lastDoneDate: oneOff ? null : toDate(lastDate),
      lastDoneKm: oneOff ? null : toInt(lastKm),
      dueDate: oneOff ? toDate(dueDate) : null,
      dueKm: oneOff ? toInt(dueKm) : null,
    });
  };

  const intervalProps = { min: 1, step: 1 };
  const kmProps = { min: 0, step: 1 };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={handleSubmit}>
        <DialogTitle>Ajouter une échéance</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            select
            required
            fullWidth
            margin="dense"
            label="Type d'échéance"
            value={typeId}
            onChange={(e) => chooseType(e.target.value)}
          >
            {types.map((t) => (
              <MenuItem key={t.id} value={t.id}>
                {t.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Périodicité (mois)"
            type="number"
            fullWidth
            margin="dense"
            value={months}
            slotProps={{ htmlInput: intervalProps }}
            onChange={(e) => setMonths(e.target.value)}
          />
          <TextField
            label="Périodicité (km)"
            type="number"
            fullWidth
            margin="dense"
            value={km}
            helperText="Aucune périodicité : échéance ponctuelle"
            slotProps={{ htmlInput: intervalProps }}
            onChange={(e) => setKm(e.target.value)}
          />
          {oneOff ? (
            <>
              <TextField
                label="Date d'échéance"
                type="date"
                fullWidth
                margin="dense"
                value={dueDate}
                slotProps={{ inputLabel: { shrink: true } }}
                onChange={(e) => setDueDate(e.target.value)}
              />
              <TextField
                label="Km d'échéance"
                type="number"
                fullWidth
                margin="dense"
                value={dueKm}
                slotProps={{ htmlInput: kmProps }}
                onChange={(e) => setDueKm(e.target.value)}
              />
            </>
          ) : (
            <>
              <TextField
                label="Dernière réalisation (date)"
                type="date"
                fullWidth
                margin="dense"
                value={lastDate}
                slotProps={{ inputLabel: { shrink: true }, htmlInput: { max: todayLocalIso() } }}
                onChange={(e) => setLastDate(e.target.value)}
              />
              <TextField
                label="Dernière réalisation (km)"
                type="number"
                fullWidth
                margin="dense"
                value={lastKm}
                slotProps={{ htmlInput: kmProps }}
                onChange={(e) => setLastKm(e.target.value)}
              />
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={submitting || typeId === ""}>
            Ajouter
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
