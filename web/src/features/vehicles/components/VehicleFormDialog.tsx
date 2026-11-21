import { Alert, Stack, TextField } from "@mui/material";
import { useState } from "react";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import useFormErrors from "../../../shared/hooks/useFormErrors";
import CompanySelect from "../../companies/components/CompanySelect";
import type { Company } from "../../companies/types/company";
import VehicleTypeSelect from "../../vehicle-types/components/VehicleTypeSelect";
import { ownerFieldFeedback } from "../api/vehicleErrors";
import { emptyVehicleForm, type VehicleFormValues } from "./vehicleForm";

interface VehicleFormDialogProps {
  open: boolean;
  /** Valeurs initiales : absent en création. En modification, l'immatriculation n'est pas modifiable. */
  initialValues?: VehicleFormValues;
  /** Type actuel du véhicule en modification : reste affiché même s'il est désactivé. */
  currentType?: { id: string; name: string } | null;
  /** Propriétaire actuel en modification : reste affiché même si sa société est inactive. */
  currentOwner?: Company | null;
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
  // Une fois un propriétaire posé, on ne peut plus le vider.
  const ownerRemovable = !initialValues?.ownerCompanyId;
  const lock = ownerOnly;
  const isEdit = initialValues !== undefined;
  const [values, setValues] = useState<VehicleFormValues>(initialValues ?? emptyVehicleForm);
  const ownerMissing = ownerOnly && values.ownerCompanyId === "";
  const feedback = ownerFieldFeedback(error, ownerOnly, ownerMissing);

  const { validate, clear, fieldProps } = useFormErrors<keyof VehicleFormValues>();

  const field = (name: keyof VehicleFormValues) => ({
    name,
    value: values[name],
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => {
      setValues((current) => ({ ...current, [name]: e.target.value }));
      clear(name);
    },
  });

  const handleSubmit = () => {
    const valid = validate({
      licensePlate: !isEdit && values.licensePlate.trim() === "" && "Indiquez l'immatriculation",
    });
    // Propriétaire seul : le message « Choisissez la société propriétaire » est déjà affiché sous le champ.
    if (valid && !ownerMissing) onSubmit(values);
  };

  return (
    <FormDialog
      open={open}
      title={isEdit ? "Modifier le véhicule" : "Ajouter un véhicule"}
      submitLabel="Enregistrer"
      submitting={submitting}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <Stack gap={2} marginTop={1}>
        {ownerOnly && <Alert severity="info">Véhicule sorti du parc : seul le propriétaire peut être renseigné.</Alert>}
        {feedback.banner && (
          <Alert severity="error" role="alert">
            {feedback.banner}
          </Alert>
        )}
        <TextField
          label="Immatriculation"
          placeholder="AB-123-CD"
          required
          disabled={isEdit || lock}
          {...field("licensePlate")}
          {...fieldProps("licensePlate")}
        />
        <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
          <TextField label="Marque" fullWidth disabled={lock} {...field("brand")} />
          <TextField label="Modèle" fullWidth disabled={lock} {...field("model")} />
        </Stack>
        <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
          <FrenchDateField
            layout="form"
            label="Mise en circulation"
            disabled={lock}
            value={values.firstRegistrationDate}
            onChange={(firstRegistrationDate) => setValues((current) => ({ ...current, firstRegistrationDate }))}
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
          allowEmpty={ownerRemovable && !ownerOnly}
          current={currentOwner}
          required={ownerOnly}
          error={feedback.hasError}
          helperText={feedback.helperText}
          autoFocus={ownerOnly}
        />
      </Stack>
    </FormDialog>
  );
}
