import { Checkbox, FormControl, FormControlLabel, FormGroup, FormHelperText, Typography } from "@mui/material";
import { useState } from "react";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import useFormErrors from "../../../shared/hooks/useFormErrors";
import { todayLocalIso } from "../../mileage/types/date";
import { useVehicleTypes } from "../../vehicle-types/api/vehicleTypeQueries";
import { deadlineTypeErrorMessage } from "../api/deadlineTypeErrors";
import { useAssignDeadlineType } from "../api/deadlineTypeQueries";
import type { DeadlineType } from "../types/deadlineType";
import AssignDeadlineTypeReport from "./AssignDeadlineTypeReport";

interface AssignDeadlineTypeDialogProps {
  type: DeadlineType;
  onClose: () => void;
}

/** Ajoute un type d'échéance (périodicité par défaut) aux véhicules d'un ou plusieurs types de véhicule, puis affiche le bilan. */
export default function AssignDeadlineTypeDialog({ type, onClose }: AssignDeadlineTypeDialogProps) {
  const vehicleTypes = useVehicleTypes(false);
  const assign = useAssignDeadlineType(type.id);
  const [selected, setSelected] = useState<string[]>([]);
  const [lastDoneDate, setLastDoneDate] = useState("");
  const { errors, validate, clear } = useFormErrors<"vehicleTypeIds">();

  const toggle = (id: string) => {
    clear("vehicleTypeIds");
    setSelected((current) => (current.includes(id) ? current.filter((x) => x !== id) : [...current, id]));
  };

  const handleSubmit = () => {
    if (!validate({ vehicleTypeIds: selected.length === 0 && "Choisissez au moins un type de véhicule" })) return;
    assign.mutate({
      vehicleTypeIds: selected,
      lastDoneDate: lastDoneDate === "" ? null : lastDoneDate,
    });
  };

  return (
    <FormDialog
      title={`Ajouter « ${type.name} » à des véhicules`}
      submitLabel="Ajouter"
      maxWidth="xs"
      done={assign.isSuccess}
      submitting={assign.isPending}
      error={assign.isError ? deadlineTypeErrorMessage(assign.error) : null}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      {assign.isSuccess ? (
        <AssignDeadlineTypeReport result={assign.data} />
      ) : (
        <>
          <Typography variant="body2" gutterBottom>
            Une échéance avec la périodicité par défaut du type est créée pour chaque véhicule en service des types
            choisis. Les véhicules qui ont déjà ce type en cours et ceux sortis du parc sont ignorés.
          </Typography>
          <FormControl component="fieldset" error={errors.vehicleTypeIds !== undefined}>
            <FormGroup>
              {(vehicleTypes.data ?? []).map((vehicleType) => (
                <FormControlLabel
                  key={vehicleType.id}
                  label={vehicleType.name}
                  control={
                    <Checkbox
                      name="vehicleTypeIds"
                      checked={selected.includes(vehicleType.id)}
                      onChange={() => toggle(vehicleType.id)}
                    />
                  }
                />
              ))}
            </FormGroup>
            {errors.vehicleTypeIds && <FormHelperText role="alert">{errors.vehicleTypeIds}</FormHelperText>}
          </FormControl>
          <FrenchDateField
            layout="form"
            label="Dernière réalisation"
            maxDate={todayLocalIso()}
            value={lastDoneDate}
            onChange={setLastDoneDate}
            helperText="Facultatif : aujourd'hui par défaut. Le km actuel du véhicule est utilisé pour un critère kilométrique."
          />
        </>
      )}
    </FormDialog>
  );
}
