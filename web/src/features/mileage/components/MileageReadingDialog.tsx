import { Alert, TextField } from "@mui/material";
import { useState } from "react";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import useFormErrors from "../../../shared/hooks/useFormErrors";
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

type Field = "kilometers" | "date" | "reason";

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
  const { validate, clear, fieldProps } = useFormErrors<Field>();

  const trimmedReason = reason.trim();

  const invalidateConfirmation = () => {
    if (confirmationRequired) onConfirmationInvalidated();
  };

  const handleSubmit = () => {
    const km = kilometers.trim();
    const valid = validate({
      kilometers: km === "" ? "Indiquez le kilométrage" : Number(km) < 0 && "Le kilométrage ne peut pas être négatif",
      date: date === "" && "Indiquez la date du relevé",
      reason: reasonRequired && trimmedReason === "" && "Indiquez le motif",
    });
    if (!valid) return;
    onSubmit({
      date,
      kilometers: Number(kilometers),
      reason: trimmedReason === "" ? null : trimmedReason,
      confirmHighMileage: confirmationRequired,
    });
  };

  return (
    <FormDialog
      title={reading ? "Modifier le relevé kilométrique" : "Saisir un relevé kilométrique"}
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
      <TextField
        label="Kilométrage"
        type="number"
        required
        fullWidth
        margin="dense"
        value={kilometers}
        slotProps={{ htmlInput: { min: 0, step: 1, inputMode: "numeric" } }}
        onChange={(e) => {
          setKilometers(e.target.value);
          clear("kilometers");
          invalidateConfirmation();
        }}
        {...fieldProps("kilometers")}
      />
      <FrenchDateField
        layout="form"
        label="Date du relevé"
        required
        maxDate={todayLocalIso()}
        value={date}
        onChange={(value) => {
          setDate(value);
          clear("date");
          invalidateConfirmation();
        }}
        {...fieldProps("date")}
      />
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
    </FormDialog>
  );
}
