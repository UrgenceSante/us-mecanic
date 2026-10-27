import {
  Alert,
  Button,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import SpeedIcon from "@mui/icons-material/Speed";
import { useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { VehicleStatus } from "../../vehicles/types/vehicle";
import {
  HIGH_MILEAGE_CONFIRMATION_REQUIRED,
  REASON_REQUIRED,
  mileageErrorCode,
  mileageErrorMessage,
} from "../api/mileageErrors";
import {
  useAddMileageReading,
  useDeleteMileageReading,
  useMileageReadings,
  useUpdateMileageReading,
} from "../api/mileageQueries";
import type { MileageReading, MileageReadingInput } from "../types/mileageReading";
import DeleteReadingDialog from "./DeleteReadingDialog";
import { formatKm } from "../types/format";
import MileageHistory from "./MileageHistory";
import MileageReadingDialog from "./MileageReadingDialog";

interface VehicleMileageProps {
  vehicleId: string;
  status: VehicleStatus;
  currentMileage: number | null;
  currentMileageDate: string | null;
}


const currentLabel = (km: number | null, date: string | null) =>
  km == null ? "Aucun relevé" : `${formatKm(km)} au ${formatDateOnly(date)}`;

export default function VehicleMileage({
  vehicleId,
  status,
  currentMileage,
  currentMileageDate,
}: VehicleMileageProps) {
  const { notifySuccess } = useNotifSnack();
  const [open, setOpen] = useState(false);
  const readings = useMileageReadings(vehicleId);
  const [editing, setEditing] = useState<MileageReading | null>(null);
  const [deleting, setDeleting] = useState<MileageReading | null>(null);
  const add = useAddMileageReading(vehicleId);
  const update = useUpdateMileageReading(vehicleId);
  const remove = useDeleteMileageReading(vehicleId);
  // Saisie et correction partagent le même formulaire et les mêmes flux d'erreur.
  const save = editing ? update : add;

  const errorCode = save.isError ? mileageErrorCode(save.error) : undefined;
  const reasonRequired = errorCode === REASON_REQUIRED;
  const confirmationRequired = errorCode === HIGH_MILEAGE_CONFIRMATION_REQUIRED;
  const dialogError =
    save.isError && !reasonRequired && !confirmationRequired
      ? mileageErrorMessage(save.error)
      : null;

  const close = () => {
    setOpen(false);
    setEditing(null);
    add.reset();
    update.reset();
  };

  const submit = (input: MileageReadingInput) => {
    const onSuccess = () => {
      notifySuccess(editing ? "Relevé modifié" : "Relevé enregistré");
      close();
    };
    if (editing) update.mutate({ readingId: editing.id, input }, { onSuccess });
    else add.mutate(input, { onSuccess });
  };

  const closeDelete = () => {
    setDeleting(null);
    remove.reset();
  };

  const confirmDelete = () => {
    if (!deleting) return;
    remove.mutate(deleting.id, {
      onSuccess: () => {
        notifySuccess("Relevé supprimé");
        closeDelete();
      },
    });
  };

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
          <Typography variant="h6">Kilométrage</Typography>
          <Stack flexGrow={1} />
          {status === "Retired" ? (
            <Typography variant="body2" color="text.secondary">
              Saisie indisponible : véhicule sorti du parc
            </Typography>
          ) : (
            <Button startIcon={<SpeedIcon />} onClick={() => setOpen(true)}>
              Saisir un relevé
            </Button>
          )}
        </Stack>
        <Typography>
          {currentLabel(currentMileage, currentMileageDate)}
        </Typography>
        {readings.isError && (
          <Alert severity="error">{mileageErrorMessage(readings.error)}</Alert>
        )}
        {readings.data && readings.data.length > 0 && (
          <MileageHistory
            readings={readings.data}
            editable={status !== "Retired"}
            onEdit={setEditing}
            onDelete={setDeleting}
          />
        )}
      </Stack>
      {(open || editing) && (
        <MileageReadingDialog
          reading={editing ?? undefined}
          submitting={save.isPending}
          error={dialogError}
          reasonRequired={reasonRequired}
          confirmationRequired={confirmationRequired}
          onSubmit={submit}
          onConfirmationInvalidated={() => save.reset()}
          onClose={close}
        />
      )}
      {deleting && (
        <DeleteReadingDialog
          reading={deleting}
          submitting={remove.isPending}
          error={remove.isError ? mileageErrorMessage(remove.error) : null}
          onConfirm={confirmDelete}
          onClose={closeDelete}
        />
      )}
    </Paper>
  );
}
