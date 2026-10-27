import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from "@mui/material";
import { useState, type FormEvent } from "react";
import { todayLocalIso } from "../types/date";
import type { MileageReading, MileageReadingInput } from "../types/mileageReading";

interface MileageReadingDialogProps {
  /** Relevé à corriger : le formulaire est pré-rempli et passe en mode modification. */
  reading?: MileageReading;
  submitting: boolean;
  /** Message d'erreur API à afficher (hors cas motif / confirmation, gérés par le formulaire). */
  error: string | null;
  /** L'API a signalé une incohérence : le motif devient obligatoire. */
  reasonRequired: boolean;
  /** L'API a signalé plus de 1 000 km/jour : demande de confirmation. */
  confirmationRequired: boolean;
  onSubmit: (input: MileageReadingInput) => void;
  /** Le km ou la date a changé alors qu'une confirmation était demandée : elle n'est plus valable. */
  onConfirmationInvalidated: () => void;
  onClose: () => void;
}

export default function MileageReadingDialog({
  reading,
  submitting,
  error,
  reasonRequired,
  confirmationRequired,
  onSubmit,
  onConfirmationInvalidated,
  onClose,
}: MileageReadingDialogProps) {
  const [kilometers, setKilometers] = useState(reading ? String(reading.kilometers) : "");
  const [date, setDate] = useState(reading?.date ?? todayLocalIso());
  const [reason, setReason] = useState(reading?.reason ?? "");
  const [reasonTouched, setReasonTouched] = useState(false);

  const trimmedReason = reason.trim();
  const reasonMissing = reasonRequired && trimmedReason === "";

  const invalidateConfirmation = () => {
    if (confirmationRequired) onConfirmationInvalidated();
  };

  const send = (confirmHighMileage: boolean) =>
    onSubmit({
      date,
      kilometers: Number(kilometers),
      reason: trimmedReason === "" ? null : trimmedReason,
      confirmHighMileage,
    });

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setReasonTouched(true);
    if (reasonMissing) return;
    send(false);
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={handleSubmit}>
        <DialogTitle>
          {reading ? "Modifier le relevé kilométrique" : "Saisir un relevé kilométrique"}
        </DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          {reasonRequired && (
            <Alert severity="info" sx={{ marginBottom: 2 }}>
              Ce kilométrage est incohérent avec les relevés voisins : un motif
              est obligatoire.
            </Alert>
          )}
          {confirmationRequired && (
            <Alert severity="warning" sx={{ marginBottom: 2 }}>
              Plus de 1 000 km/jour en moyenne depuis le relevé précédent.
              Vérifiez la valeur saisie, puis confirmez pour l'enregistrer quand
              même.
            </Alert>
          )}
          <TextField
            label="Kilométrage"
            type="number"
            required
            autoFocus
            fullWidth
            margin="dense"
            value={kilometers}
            slotProps={{ htmlInput: { min: 0, step: 1 } }}
            onChange={(e) => {
              setKilometers(e.target.value);
              invalidateConfirmation();
            }}
          />
          <TextField
            label="Date du relevé"
            type="date"
            required
            fullWidth
            margin="dense"
            value={date}
            slotProps={{
              inputLabel: { shrink: true },
              htmlInput: { max: todayLocalIso() },
            }}
            onChange={(e) => {
              setDate(e.target.value);
              invalidateConfirmation();
            }}
          />
          <TextField
            label="Motif"
            fullWidth
            margin="dense"
            multiline
            required={reasonRequired}
            value={reason}
            error={reasonTouched && reasonMissing}
            helperText={
              reasonRequired
                ? "Obligatoire : valeur incohérente avec les relevés voisins"
                : "Facultatif"
            }
            slotProps={{ htmlInput: { maxLength: 500 } }}
            onChange={(e) => setReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          {confirmationRequired ? (
            <Button
              variant="contained"
              color="warning"
              disabled={submitting || reasonMissing}
              onClick={() => send(true)}
            >
              Confirmer et enregistrer
            </Button>
          ) : (
            <Button type="submit" variant="contained" disabled={submitting}>
              Enregistrer
            </Button>
          )}
        </DialogActions>
      </form>
    </Dialog>
  );
}
