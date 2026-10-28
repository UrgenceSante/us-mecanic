import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";
import GarageSelect from "../../garages/components/GarageSelect";
import { todayLocalIso } from "../../mileage/types/date";
import type { DeadlineCompletionInput, VehicleDeadline } from "../types/vehicleDeadline";

interface CompleteDeadlineDialogProps {
  deadline: VehicleDeadline;
  /** Km actuel du véhicule, proposé par défaut. */
  currentMileage: number | null;
  submitting: boolean;
  error: string | null;
  reasonRequired: boolean;
  confirmationRequired: boolean;
  onSubmit: (input: DeadlineCompletionInput) => void;
  /** Le km ou la date a changé alors qu'une confirmation était demandée : elle n'est plus valable. */
  onConfirmationInvalidated: () => void;
  onClose: () => void;
}

export default function CompleteDeadlineDialog({
  deadline,
  currentMileage,
  submitting,
  error,
  reasonRequired,
  confirmationRequired,
  onSubmit,
  onConfirmationInvalidated,
  onClose,
}: CompleteDeadlineDialogProps) {
  const [date, setDate] = useState(todayLocalIso());
  const prefilledKm = currentMileage === null ? "" : String(currentMileage);
  const [kilometers, setKilometers] = useState(prefilledKm);
  const [garageId, setGarageId] = useState<string | null>(null);
  const [comment, setComment] = useState("");
  const [reason, setReason] = useState("");
  const [reasonTouched, setReasonTouched] = useState(false);

  const kmRequired = deadline.intervalKm !== null || deadline.dueKm !== null;
  const trimmedReason = reason.trim();
  const reasonMissing = reasonRequired && trimmedReason === "";

  const invalidateConfirmation = () => {
    if (confirmationRequired) onConfirmationInvalidated();
  };

  const send = (confirmHighMileage: boolean) =>
    onSubmit({
      date,
      kilometers: kilometers.trim() === "" ? null : Number(kilometers),
      garageId,
      comment: comment.trim() === "" ? null : comment.trim(),
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
        <DialogTitle>Marquer réalisée : {deadline.typeName ?? "échéance"}</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          {reasonRequired && (
            <Alert severity="info" sx={{ marginBottom: 2 }}>
              Ce kilométrage est incohérent avec les relevés voisins : un motif est obligatoire.
            </Alert>
          )}
          {confirmationRequired && (
            <Alert severity="warning" sx={{ marginBottom: 2 }}>
              Plus de 1 000 km/jour en moyenne depuis le relevé précédent. Vérifiez la valeur saisie, puis confirmez
              pour l'enregistrer quand même.
            </Alert>
          )}
          <TextField
            label="Date de réalisation"
            type="date"
            required
            fullWidth
            margin="dense"
            value={date}
            slotProps={{ inputLabel: { shrink: true }, htmlInput: { max: todayLocalIso() } }}
            onChange={(e) => {
              setDate(e.target.value);
              // Le km prérempli est celui d'aujourd'hui : faux pour une date passée, on le vide.
              if (e.target.value !== todayLocalIso() && kilometers === prefilledKm) setKilometers("");
              invalidateConfirmation();
            }}
          />
          <TextField
            label="Kilométrage"
            type="number"
            required={kmRequired}
            fullWidth
            margin="dense"
            value={kilometers}
            helperText={kmRequired ? "Obligatoire : l'échéance a un critère kilométrique" : "Facultatif"}
            slotProps={{ htmlInput: { min: 0, step: 1 } }}
            onChange={(e) => {
              setKilometers(e.target.value);
              invalidateConfirmation();
            }}
          />
          <div style={{ marginTop: 8, marginBottom: 4 }}>
            <GarageSelect value={garageId} onChange={setGarageId} />
          </div>
          <TextField
            label="Commentaire"
            fullWidth
            margin="dense"
            multiline
            value={comment}
            slotProps={{ htmlInput: { maxLength: 500 } }}
            onChange={(e) => setComment(e.target.value)}
          />
          {reasonRequired && (
            <TextField
              label="Motif"
              fullWidth
              margin="dense"
              multiline
              required
              value={reason}
              error={reasonTouched && reasonMissing}
              helperText="Motif du relevé kilométrique"
              slotProps={{ htmlInput: { maxLength: 500 } }}
              onChange={(e) => setReason(e.target.value)}
            />
          )}
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
