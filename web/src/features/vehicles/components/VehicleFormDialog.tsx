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
import CompanySelect from "../../companies/components/CompanySelect";
import VehicleTypeSelect from "../../vehicle-types/components/VehicleTypeSelect";
import { vehicleErrorMessage, vehicleOwnerErrorMessage } from "../api/vehicleErrors";
import { emptyVehicleForm, type VehicleFormValues } from "./vehicleForm";

interface VehicleFormDialogProps {
  open: boolean;
  /** Valeurs initiales : absent en création. En modification, l'immatriculation n'est pas modifiable. */
  initialValues?: VehicleFormValues;
  /** Type actuel du véhicule en modification : reste affiché même s'il est désactivé. */
  currentType?: { id: string; name: string } | null;
  /** Propriétaire actuel en modification : reste affiché même si sa société est inactive. */
  currentOwner?: { id: string; name: string } | null;
  /** Véhicule sorti du parc sans propriétaire : seul le propriétaire est éditable. */
  ownerOnly?: boolean;
  submitting: boolean;
  /** Erreur de l'enregistrement (axios) : affichée à côté du champ concerné, sinon en bandeau. */
  error: unknown;
  onSubmit: (values: VehicleFormValues) => void;
  onClose: () => void;
}

export default function VehicleFormDialog(props: VehicleFormDialogProps) {
  const { open, initialValues, currentType, currentOwner, ownerOnly = false, submitting, error, onSubmit, onClose } = props;
  const ownerError = vehicleOwnerErrorMessage(error);
  const generalError = error && !ownerError ? vehicleErrorMessage(error) : null;
  // Une fois un propriétaire posé, on ne peut plus le vider.
  const ownerRemovable = !initialValues?.ownerCompanyId;
  const lock = ownerOnly;
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
            {ownerOnly && (
              <Alert severity="info">Véhicule sorti du parc : seul le propriétaire peut être renseigné.</Alert>
            )}
            {generalError && <Alert severity="error">{generalError}</Alert>}
            <TextField
              label="Immatriculation"
              placeholder="AB-123-CD"
              required
              disabled={isEdit || lock}
              autoFocus={!isEdit}
              {...field("licensePlate")}
            />
            <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
              <TextField label="Marque" fullWidth disabled={lock} {...field("brand")} />
              <TextField label="Modèle" fullWidth disabled={lock} {...field("model")} />
            </Stack>
            <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
              <TextField
                label="Mise en circulation"
                type="date"
                fullWidth
                disabled={lock}
                slotProps={{ inputLabel: { shrink: true } }}
                {...field("firstRegistrationDate")}
              />
              <TextField label="N° interne / indicatif" fullWidth disabled={lock} {...field("internalNumber")} />
            </Stack>
            <TextField label="VIN (n° de série)" disabled={lock} {...field("vin")} />
            <VehicleTypeSelect
              value={values.typeId}
              onChange={(typeId) => setValues((current) => ({ ...current, typeId }))}
              emptyLabel="Aucun"
              current={currentType}
              disabled={lock}
            />
            <CompanySelect
              label="Société propriétaire"
              value={values.ownerCompanyId}
              onChange={(ownerCompanyId) => setValues((current) => ({ ...current, ownerCompanyId }))}
              emptyLabel="À renseigner"
              allowEmpty={ownerRemovable}
              current={currentOwner}
              required={ownerOnly}
              error={ownerError !== null}
              helperText={ownerError ?? undefined}
              autoFocus={ownerOnly}
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
