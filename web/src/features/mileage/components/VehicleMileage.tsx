import {
  Alert,
  Button,
  List,
  ListItem,
  ListItemText,
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
  useMileageReadings,
} from "../api/mileageQueries";
import type { MileageReadingInput } from "../types/mileageReading";
import MileageReadingDialog from "./MileageReadingDialog";

interface VehicleMileageProps {
  vehicleId: string;
  status: VehicleStatus;
  currentMileage: number | null;
  currentMileageDate: string | null;
}

const formatKm = (km: number) => `${km.toLocaleString("fr-FR")} km`;

export default function VehicleMileage({
  vehicleId,
  status,
  currentMileage,
  currentMileageDate,
}: VehicleMileageProps) {
  const { notifySuccess } = useNotifSnack();
  const [open, setOpen] = useState(false);
  const readings = useMileageReadings(vehicleId);
  const add = useAddMileageReading(vehicleId);

  const errorCode = add.isError ? mileageErrorCode(add.error) : undefined;
  const reasonRequired = errorCode === REASON_REQUIRED;
  const confirmationRequired = errorCode === HIGH_MILEAGE_CONFIRMATION_REQUIRED;
  const dialogError =
    add.isError && !reasonRequired && !confirmationRequired
      ? mileageErrorMessage(add.error)
      : null;

  const close = () => {
    setOpen(false);
    add.reset();
  };

  const submit = (input: MileageReadingInput) =>
    add.mutate(input, {
      onSuccess: () => {
        notifySuccess("Relevé enregistré");
        close();
      },
    });

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
          {currentMileage == null
            ? "Aucun relevé"
            : `${formatKm(currentMileage)} au ${formatDateOnly(currentMileageDate)}`}
        </Typography>
        {readings.isError && (
          <Alert severity="error">{mileageErrorMessage(readings.error)}</Alert>
        )}
        {readings.data && readings.data.length > 0 && (
          <List dense aria-label="Historique des relevés">
            {readings.data.map((r) => (
              <ListItem key={r.id} disableGutters>
                <ListItemText
                  primary={`${formatDateOnly(r.date)} : ${formatKm(r.kilometers)}`}
                  secondary={r.reason ? `Motif : ${r.reason}` : undefined}
                />
              </ListItem>
            ))}
          </List>
        )}
      </Stack>
      {open && (
        <MileageReadingDialog
          submitting={add.isPending}
          error={dialogError}
          reasonRequired={reasonRequired}
          confirmationRequired={confirmationRequired}
          onSubmit={submit}
          onConfirmationInvalidated={() => add.reset()}
          onClose={close}
        />
      )}
    </Paper>
  );
}
