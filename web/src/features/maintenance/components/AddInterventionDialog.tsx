import { Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";
import AmountField from "../../../shared/components/AmountField";
import { parseAmount } from "../../../shared/format/parseAmount";
import GarageSelect from "../../garages/components/GarageSelect";
import { todayLocalIso } from "../../mileage/types/date";
import type { VehicleDeadline } from "../../vehicle-deadlines/types/vehicleDeadline";
import type { InterventionInput } from "../types/intervention";
import DeadlineChecklist from "./DeadlineChecklist";
import InterventionAlerts from "./InterventionAlerts";

interface AddInterventionDialogProps {
  /** Échéances en cours du véhicule (non closes). */
  deadlines: VehicleDeadline[];
  currentMileage: number | null;
  submitting: boolean;
  error: string | null;
  reasonRequired: boolean;
  confirmationRequired: boolean;
  onSubmit: (input: InterventionInput) => void;
  /** Le km ou la date a changé alors qu'une confirmation était demandée : elle n'est plus valable. */
  onConfirmationInvalidated: () => void;
  onClose: () => void;
}

export default function AddInterventionDialog(props: AddInterventionDialogProps) {
  const { deadlines, currentMileage, submitting, error, reasonRequired, confirmationRequired } = props;
  const [date, setDate] = useState(todayLocalIso());
  const prefilledKm = currentMileage == null ? "" : String(currentMileage);
  const [kilometers, setKilometers] = useState(prefilledKm);
  const [garageId, setGarageId] = useState<string | null>(null);
  const [work, setWork] = useState("");
  const [checked, setChecked] = useState<string[]>([]);
  const [reason, setReason] = useState("");
  const [amount, setAmount] = useState("");

  const trimmedReason = reason.trim();
  const reasonMissing = reasonRequired && trimmedReason === "";
  const kilometersRequired = deadlines.some(
    (d) => checked.includes(d.id) && (d.intervalKm != null || d.dueKm != null),
  );
  const kilometersMissing = kilometersRequired && kilometers.trim() === "";
  const parsedAmount = parseAmount(amount);
  const invalid = work.trim() === "" || kilometersMissing || reasonMissing || parsedAmount.error !== null;

  const invalidateConfirmation = () => {
    if (confirmationRequired) props.onConfirmationInvalidated();
  };

  const send = (confirmHighMileage: boolean) =>
    props.onSubmit({
      date,
      kilometers: kilometers.trim() === "" ? null : Number(kilometers),
      garageId,
      work: work.trim(),
      amount: parsedAmount.value,
      deadlineIds: checked,
      reason: trimmedReason === "" ? null : trimmedReason,
      confirmHighMileage,
    });

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!invalid) send(false);
  };

  const toggle = (id: string) =>
    setChecked((c) => (c.includes(id) ? c.filter((x) => x !== id) : [...c, id]));

  return (
    <Dialog open onClose={props.onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit}>
        <DialogTitle>Ajouter une intervention</DialogTitle>
        <DialogContent>
          <InterventionAlerts error={error} reasonRequired={reasonRequired} confirmationRequired={confirmationRequired} />
          <TextField
            label="Date de l'intervention"
            type="date"
            required
            fullWidth
            margin="dense"
            value={date}
            slotProps={{ inputLabel: { shrink: true }, htmlInput: { max: todayLocalIso() } }}
            onChange={(e) => {
              setDate(e.target.value);
              // Le km prérempli est celui d'aujourd'hui : faux pour une intervention passée, on le vide.
              if (e.target.value !== todayLocalIso() && kilometers === prefilledKm) setKilometers("");
              invalidateConfirmation();
            }}
          />
          <TextField
            label="Kilométrage"
            type="number"
            required={kilometersRequired}
            fullWidth
            margin="dense"
            value={kilometers}
            helperText={kilometersRequired ? "Obligatoire : une échéance cochée a un critère kilométrique" : "Facultatif"}
            slotProps={{ htmlInput: { min: 0, step: 1 } }}
            onChange={(e) => {
              setKilometers(e.target.value);
              invalidateConfirmation();
            }}
          />
          <div style={{ marginTop: 8, marginBottom: 4 }}>
            <GarageSelect value={garageId} onChange={setGarageId} />
          </div>
          <AmountField value={amount} onChange={setAmount} />
          <TextField
            label="Travaux réalisés"
            required
            multiline
            minRows={2}
            fullWidth
            margin="dense"
            value={work}
            slotProps={{ htmlInput: { maxLength: 2000 } }}
            onChange={(e) => setWork(e.target.value)}
          />
          <DeadlineChecklist deadlines={deadlines} checked={checked} onToggle={toggle} />
          {(reasonRequired || reason !== "") && (
            <TextField
              label="Motif"
              fullWidth
              margin="dense"
              multiline
              required={reasonRequired}
              value={reason}
              helperText={reasonRequired ? "Obligatoire : valeur incohérente avec les relevés voisins" : "Facultatif"}
              slotProps={{ htmlInput: { maxLength: 500 } }}
              onChange={(e) => setReason(e.target.value)}
            />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={props.onClose}>Annuler</Button>
          {confirmationRequired ? (
            <Button variant="contained" color="warning" disabled={submitting || invalid} onClick={() => send(true)}>
              Confirmer et enregistrer
            </Button>
          ) : (
            <Button type="submit" variant="contained" disabled={submitting || invalid}>
              Enregistrer
            </Button>
          )}
        </DialogActions>
      </form>
    </Dialog>
  );
}
