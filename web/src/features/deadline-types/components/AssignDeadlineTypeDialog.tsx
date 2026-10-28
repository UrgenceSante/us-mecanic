import {
  Alert,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  FormGroup,
  TextField,
  Typography,
} from "@mui/material";
import { useState, type FormEvent } from "react";
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

  const toggle = (id: string) =>
    setSelected((current) => (current.includes(id) ? current.filter((x) => x !== id) : [...current, id]));

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    assign.mutate({
      vehicleTypeIds: selected,
      lastDoneDate: lastDoneDate === "" ? null : lastDoneDate,
    });
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={handleSubmit}>
        <DialogTitle>Ajouter « {type.name} » à des véhicules</DialogTitle>
        <DialogContent>
          {assign.isSuccess ? (
            <AssignDeadlineTypeReport result={assign.data} />
          ) : (
            <>
              {assign.isError && (
                <Alert severity="error" sx={{ marginBottom: 2 }}>
                  {deadlineTypeErrorMessage(assign.error)}
                </Alert>
              )}
              <Typography variant="body2" gutterBottom>
                Une échéance avec la périodicité par défaut du type est créée pour chaque véhicule en service des types
                choisis. Les véhicules qui ont déjà ce type en cours et ceux sortis du parc sont ignorés.
              </Typography>
              <FormGroup>
                {(vehicleTypes.data ?? []).map((vehicleType) => (
                  <FormControlLabel
                    key={vehicleType.id}
                    label={vehicleType.name}
                    control={
                      <Checkbox checked={selected.includes(vehicleType.id)} onChange={() => toggle(vehicleType.id)} />
                    }
                  />
                ))}
              </FormGroup>
              <TextField
                label="Dernière réalisation"
                type="date"
                fullWidth
                margin="dense"
                value={lastDoneDate}
                helperText="Facultatif : aujourd'hui par défaut. Le km actuel du véhicule est utilisé pour un critère kilométrique."
                slotProps={{
                  inputLabel: { shrink: true },
                  htmlInput: { max: todayLocalIso() },
                }}
                onChange={(e) => setLastDoneDate(e.target.value)}
              />
            </>
          )}
        </DialogContent>
        <DialogActions>
          {assign.isSuccess ? (
            <Button variant="contained" onClick={onClose}>
              Fermer
            </Button>
          ) : (
            <>
              <Button onClick={onClose}>Annuler</Button>
              <Button type="submit" variant="contained" disabled={assign.isPending || selected.length === 0}>
                Ajouter
              </Button>
            </>
          )}
        </DialogActions>
      </form>
    </Dialog>
  );
}
