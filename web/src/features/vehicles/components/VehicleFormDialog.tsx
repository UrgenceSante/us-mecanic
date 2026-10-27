import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { useState, type FormEvent } from "react";
import VehicleTypeSelect from "../../vehicle-types/components/VehicleTypeSelect";
import { emptyVehicleForm, type VehicleFormValues } from "./vehicleForm";

interface VehicleFormDialogProps {
  open: boolean;
  /** Valeurs initiales : absent en création. En modification, l'immatriculation n'est pas modifiable. */
  initialValues?: VehicleFormValues;
  /** Type actuel du véhicule en modification : reste affiché même s'il est désactivé. */
  currentType?: { id: string; name: string } | null;
  submitting: boolean;
  error: string | null;
  onSubmit: (values: VehicleFormValues) => void;
  onClose: () => void;
}

export default function VehicleFormDialog(props: VehicleFormDialogProps) {
  const { open, initialValues, currentType, submitting, error, onSubmit, onClose } = props;
  const isEdit = initialValues !== undefined;
  const [values, setValues] = useState<VehicleFormValues>(initialValues ?? emptyVehicleForm);

  const field = (name: keyof VehicleFormValues) => ({
    name,
    value: values[name],
    onChange: (e: React.ChangeEvent<HTMLInputElement>) =>
      setValues((current) => ({ ...current, [name]: e.target.value })),
  });

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit(values);
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{isEdit ? "Modifier le véhicule" : "Ajouter un véhicule"}</DialogTitle>
        <DialogContent>
          <Stack gap={2} marginTop={1}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              label="Immatriculation"
              placeholder="AB-123-CD"
              required
              disabled={isEdit}
              autoFocus={!isEdit}
              {...field("licensePlate")}
            />
            <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
              <TextField label="Marque" fullWidth {...field("brand")} />
              <TextField label="Modèle" fullWidth {...field("model")} />
            </Stack>
            <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
              <TextField
                label="Mise en circulation"
                type="date"
                fullWidth
                slotProps={{ inputLabel: { shrink: true } }}
                {...field("firstRegistrationDate")}
              />
              <TextField label="N° interne / indicatif" fullWidth {...field("internalNumber")} />
            </Stack>
            <TextField label="VIN (n° de série)" {...field("vin")} />
            <VehicleTypeSelect
              value={values.typeId}
              onChange={(typeId) => setValues((current) => ({ ...current, typeId }))}
              emptyLabel="Aucun"
              current={currentType}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={submitting}>
            Enregistrer
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
