import { Alert, Button, List, Paper, Stack, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import { useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { useDeadlineTypes } from "../../deadline-types/api/deadlineTypeQueries";
import type { VehicleStatus } from "../../vehicles/types/vehicle";
import { completionFlow } from "../api/completionFlow";
import { deadlineErrorMessage } from "../api/vehicleDeadlineErrors";
import { useAddVehicleDeadline, useCompleteVehicleDeadline, useVehicleDeadlines } from "../api/vehicleDeadlineQueries";
import type { DeadlineCompletionInput, VehicleDeadline, VehicleDeadlineInput } from "../types/vehicleDeadline";
import AddDeadlineDialog from "./AddDeadlineDialog";
import CompleteDeadlineDialog from "./CompleteDeadlineDialog";
import DeadlineRow from "./DeadlineRow";

interface VehicleDeadlinesProps {
  vehicleId: string;
  status: VehicleStatus;
  currentMileage: number | null;
}

export default function VehicleDeadlines({ vehicleId, status, currentMileage }: VehicleDeadlinesProps) {
  const { notifySuccess } = useNotifSnack();
  const [adding, setAdding] = useState(false);
  const [completing, setCompleting] = useState<VehicleDeadline | null>(null);
  const deadlines = useVehicleDeadlines(vehicleId);
  const types = useDeadlineTypes();
  const add = useAddVehicleDeadline(vehicleId);
  const complete = useCompleteVehicleDeadline(vehicleId, completing?.id ?? "");

  const flow = completionFlow(complete.isError ? complete.error : null);

  const closeAdd = () => {
    setAdding(false);
    add.reset();
  };
  const closeComplete = () => {
    setCompleting(null);
    complete.reset();
  };

  const submitAdd = (input: VehicleDeadlineInput) =>
    add.mutate(input, {
      onSuccess: () => {
        notifySuccess("Échéance ajoutée");
        closeAdd();
      },
    });

  const submitComplete = (input: DeadlineCompletionInput) =>
    complete.mutate(input, {
      onSuccess: () => {
        notifySuccess("Échéance réalisée");
        closeComplete();
      },
    });

  const retired = status === "Retired";

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
          <Typography variant="h6">Échéances</Typography>
          <Stack flexGrow={1} />
          {!retired && (
            <Button startIcon={<AddIcon />} onClick={() => setAdding(true)}>
              Ajouter une échéance
            </Button>
          )}
        </Stack>
        {deadlines.isError && <Alert severity="error">{deadlineErrorMessage(deadlines.error)}</Alert>}
        {deadlines.data && deadlines.data.length === 0 && <Typography>Aucune échéance</Typography>}
        {deadlines.data && deadlines.data.length > 0 && (
          <List dense aria-label="Échéances du véhicule">
            {deadlines.data.map((d) => (
              <DeadlineRow key={d.id} deadline={d} canComplete={!retired} onComplete={setCompleting} />
            ))}
          </List>
        )}
      </Stack>
      {adding && (
        <AddDeadlineDialog
          types={types.data ?? []}
          submitting={add.isPending}
          error={add.isError ? deadlineErrorMessage(add.error) : null}
          onSubmit={submitAdd}
          onClose={closeAdd}
        />
      )}
      {completing && (
        <CompleteDeadlineDialog
          deadline={completing}
          currentMileage={currentMileage}
          submitting={complete.isPending}
          error={flow.message}
          reasonRequired={flow.reasonRequired}
          confirmationRequired={flow.confirmationRequired}
          onSubmit={submitComplete}
          onConfirmationInvalidated={() => complete.reset()}
          onClose={closeComplete}
        />
      )}
    </Paper>
  );
}
