import { TextField } from "@mui/material";
import { useState } from "react";
import AmountField from "../../../shared/components/AmountField";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import { parseAmount } from "../../../shared/format/parseAmount";
import useFormErrors from "../../../shared/hooks/useFormErrors";
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

type Field = "date" | "kilometers" | "amount" | "work" | "reason";

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
  const { errors, validate, clear, fieldProps } = useFormErrors<Field>();

  const trimmedReason = reason.trim();
  const kilometersRequired = deadlines.some(
    (d) => checked.includes(d.id) && (d.intervalKm != null || d.dueKm != null),
  );
  const parsedAmount = parseAmount(amount);

  const invalidateConfirmation = () => {
    if (confirmationRequired) props.onConfirmationInvalidated();
  };

  const handleSubmit = () => {
    const km = kilometers.trim();
    const valid = validate({
      date: date === "" && "Indiquez la date de l'intervention",
      kilometers:
        km === "" ? kilometersRequired && "Indiquez le kilométrage" : Number(km) < 0 && "Le kilométrage ne peut pas être négatif",
      amount: parsedAmount.error,
      work: work.trim() === "" && "Indiquez les travaux réalisés",
      reason: reasonRequired && trimmedReason === "" && "Indiquez le motif",
    });
    if (!valid) return;
    props.onSubmit({
      date,
      kilometers: km === "" ? null : Number(km),
      garageId,
      work: work.trim(),
      amount: parsedAmount.value,
      deadlineIds: checked,
      reason: trimmedReason === "" ? null : trimmedReason,
      confirmHighMileage: confirmationRequired,
    });
  };

  const toggle = (id: string) =>
    setChecked((c) => (c.includes(id) ? c.filter((x) => x !== id) : [...c, id]));

  return (
    <FormDialog
      title="Ajouter une intervention"
      submitLabel={confirmationRequired ? "Confirmer et enregistrer" : "Enregistrer"}
      submitColor={confirmationRequired ? "warning" : "primary"}
      submitting={submitting}
      error={error}
      onSubmit={handleSubmit}
      onClose={props.onClose}
    >
      <InterventionAlerts error={null} reasonRequired={reasonRequired} confirmationRequired={confirmationRequired} />
      <FrenchDateField
        layout="form"
        label="Date de l'intervention"
        required
        maxDate={todayLocalIso()}
        value={date}
        onChange={(value) => {
          setDate(value);
          clear("date");
          // Le km prérempli est celui d'aujourd'hui : faux pour une intervention passée, on le vide.
          if (value !== todayLocalIso() && kilometers === prefilledKm) setKilometers("");
          invalidateConfirmation();
        }}
        {...fieldProps("date")}
      />
      <TextField
        label="Kilométrage"
        type="number"
        required={kilometersRequired}
        fullWidth
        margin="dense"
        value={kilometers}
        slotProps={{ htmlInput: { min: 0, step: 1, inputMode: "numeric" } }}
        onChange={(e) => {
          setKilometers(e.target.value);
          clear("kilometers");
          invalidateConfirmation();
        }}
        {...fieldProps(
          "kilometers",
          kilometersRequired ? "Obligatoire : une échéance cochée a un critère kilométrique" : "Facultatif",
        )}
      />
      <div style={{ marginTop: 8, marginBottom: 4 }}>
        <GarageSelect value={garageId} onChange={setGarageId} />
      </div>
      <AmountField
        name="amount"
        errorText={errors.amount}
        value={amount}
        onChange={(v) => {
          setAmount(v);
          clear("amount");
        }}
      />
      <TextField
        label="Travaux réalisés"
        required
        multiline
        minRows={2}
        fullWidth
        margin="dense"
        value={work}
        slotProps={{ htmlInput: { maxLength: 2000 } }}
        onChange={(e) => {
          setWork(e.target.value);
          clear("work");
        }}
        {...fieldProps("work")}
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
          slotProps={{ htmlInput: { maxLength: 500 } }}
          onChange={(e) => {
            setReason(e.target.value);
            clear("reason");
          }}
          {...fieldProps(
            "reason",
            reasonRequired ? "Obligatoire : valeur incohérente avec les relevés voisins" : "Facultatif",
          )}
        />
      )}
    </FormDialog>
  );
}
