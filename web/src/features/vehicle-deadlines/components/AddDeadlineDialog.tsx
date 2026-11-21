import { MenuItem, TextField } from "@mui/material";
import { useState } from "react";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import useFormErrors from "../../../shared/hooks/useFormErrors";
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

const intervalProps = { min: 1, step: 1, inputMode: "numeric" } as const;
const kmProps = { min: 0, step: 1, inputMode: "numeric" } as const;

export default function AddDeadlineDialog({ types, submitting, error, onSubmit, onClose }: AddDeadlineDialogProps) {
  const [typeId, setTypeId] = useState("");
  const [months, setMonths] = useState("");
  const [km, setKm] = useState("");
  const [lastDate, setLastDate] = useState("");
  const [lastKm, setLastKm] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [dueKm, setDueKm] = useState("");
  const { validate, clear, fieldProps } = useFormErrors<"typeId">();

  // Périodicité vide des deux côtés = échéance ponctuelle.
  const oneOff = months.trim() === "" && km.trim() === "";

  const chooseType = (id: string) => {
    setTypeId(id);
    clear("typeId");
    const type = types.find((t) => t.id === id);
    setMonths(toStr(type?.defaultIntervalMonths ?? null));
    setKm(toStr(type?.defaultIntervalKm ?? null));
  };

  const handleSubmit = () => {
    if (!validate({ typeId: typeId === "" && "Choisissez un type d'échéance" })) return;
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

  return (
    <FormDialog
      title="Ajouter une échéance"
      submitLabel="Ajouter"
      maxWidth="xs"
      submitting={submitting}
      error={error}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <TextField
        select
        required
        fullWidth
        margin="dense"
        label="Type d'échéance"
        value={typeId}
        onChange={(e) => chooseType(e.target.value)}
        {...fieldProps("typeId")}
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
          <FrenchDateField layout="form" label="Date d'échéance" value={dueDate} onChange={setDueDate} />
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
          <FrenchDateField
            layout="form"
            label="Dernière réalisation (date)"
            maxDate={todayLocalIso()}
            value={lastDate}
            onChange={setLastDate}
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
    </FormDialog>
  );
}
