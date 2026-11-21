import { Alert, TextField } from "@mui/material";
import { useState } from "react";
import AmountField from "../../../shared/components/AmountField";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import { parseAmount } from "../../../shared/format/parseAmount";
import useFormErrors from "../../../shared/hooks/useFormErrors";
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

type Field = "date" | "kilometers" | "amount" | "reason";

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
  const [amount, setAmount] = useState("");
  const { errors, validate, clear, fieldProps } = useFormErrors<Field>();

  const kmRequired = deadline.intervalKm !== null || deadline.dueKm !== null;
  const trimmedReason = reason.trim();
  const parsedAmount = parseAmount(amount);

  const invalidateConfirmation = () => {
    if (confirmationRequired) onConfirmationInvalidated();
  };

  const handleSubmit = () => {
    const km = kilometers.trim();
    const valid = validate({
      date: date === "" && "Indiquez la date de réalisation",
      kilometers: km === "" ? kmRequired && "Indiquez le kilométrage" : Number(km) < 0 && "Le kilométrage ne peut pas être négatif",
      amount: parsedAmount.error,
      reason: reasonRequired && trimmedReason === "" && "Indiquez le motif",
    });
    if (!valid) return;
    onSubmit({
      date,
      kilometers: km === "" ? null : Number(km),
      garageId,
      amount: parsedAmount.value,
      comment: comment.trim() === "" ? null : comment.trim(),
      reason: trimmedReason === "" ? null : trimmedReason,
      confirmHighMileage: confirmationRequired,
    });
  };

  return (
    <FormDialog
      title={`Marquer réalisée : ${deadline.typeName ?? "échéance"}`}
      submitLabel={confirmationRequired ? "Confirmer et enregistrer" : "Enregistrer"}
      submitColor={confirmationRequired ? "warning" : "primary"}
      maxWidth="xs"
      submitting={submitting}
      error={error}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      {reasonRequired && (
        <Alert severity="info" sx={{ marginBottom: 2 }}>
          Ce kilométrage est incohérent avec les relevés voisins : un motif est obligatoire.
        </Alert>
      )}
      {confirmationRequired && (
        <Alert severity="warning" sx={{ marginBottom: 2 }}>
          Plus de 1 000 km/jour en moyenne depuis le relevé précédent. Vérifiez la valeur saisie, puis confirmez pour
          l'enregistrer quand même.
        </Alert>
      )}
      <FrenchDateField
        layout="form"
        label="Date de réalisation"
        required
        maxDate={todayLocalIso()}
        value={date}
        onChange={(value) => {
          setDate(value);
          clear("date");
          // Le km prérempli est celui d'aujourd'hui : faux pour une date passée, on le vide.
          if (value !== todayLocalIso() && kilometers === prefilledKm) setKilometers("");
          invalidateConfirmation();
        }}
        {...fieldProps("date")}
      />
      <TextField
        label="Kilométrage"
        type="number"
        required={kmRequired}
        fullWidth
        margin="dense"
        value={kilometers}
        slotProps={{ htmlInput: { min: 0, step: 1, inputMode: "numeric" } }}
        onChange={(e) => {
          setKilometers(e.target.value);
          clear("kilometers");
          invalidateConfirmation();
        }}
        {...fieldProps("kilometers", kmRequired ? "Obligatoire : l'échéance a un critère kilométrique" : "Facultatif")}
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
        label="Commentaire"
        fullWidth
        margin="dense"
        multiline
        value={comment}
        helperText="Facultatif"
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
          slotProps={{ htmlInput: { maxLength: 500 } }}
          onChange={(e) => {
            setReason(e.target.value);
            clear("reason");
          }}
          {...fieldProps("reason", "Motif du relevé kilométrique")}
        />
      )}
    </FormDialog>
  );
}
