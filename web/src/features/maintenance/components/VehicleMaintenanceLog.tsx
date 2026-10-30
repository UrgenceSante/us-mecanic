import { Alert, Button, Paper, Stack, Typography } from "@mui/material";
import BuildIcon from "@mui/icons-material/Build";
import { useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { useVehicleDeadlines } from "../../vehicle-deadlines/api/vehicleDeadlineQueries";
import type { VehicleStatus } from "../../vehicles/types/vehicle";
import { interventionErrorMessage, interventionFlow } from "../api/interventionErrors";
import { useAddIntervention, useChangeInterventionAmount, useDeleteIntervention, useInterventions } from "../api/interventionQueries";
import type { Intervention, InterventionInput } from "../types/intervention";
import AddInterventionDialog from "./AddInterventionDialog";
import DeleteInterventionDialog from "./DeleteInterventionDialog";
import EditAmountDialog from "./EditAmountDialog";
import InterventionList from "./InterventionList";

interface VehicleMaintenanceLogProps {
  vehicleId: string;
  status: VehicleStatus;
  currentMileage: number | null;
}

export default function VehicleMaintenanceLog({ vehicleId, status, currentMileage }: VehicleMaintenanceLogProps) {
  const { notifySuccess } = useNotifSnack();
  const [open, setOpen] = useState(false);
  const interventions = useInterventions(vehicleId);
  const deadlines = useVehicleDeadlines(vehicleId);
  const add = useAddIntervention(vehicleId);
  const [toDelete, setToDelete] = useState<Intervention | null>(null);
  const remove = useDeleteIntervention(vehicleId);
  const [toEdit, setToEdit] = useState<Intervention | null>(null);
  const changeAmount = useChangeInterventionAmount(vehicleId);
  const flow = interventionFlow(add.isError ? add.error : null);

  const close = () => {
    setOpen(false);
    add.reset();
  };

  const closeDelete = () => {
    setToDelete(null);
    remove.reset();
  };

  const closeEdit = () => {
    setToEdit(null);
    changeAmount.reset();
  };

  const submitAmount = (amount: number | null) => {
    if (!toEdit) return;
    changeAmount.mutate(
      { interventionId: toEdit.id, amount },
      {
        onSuccess: () => {
          notifySuccess("Montant modifié");
          closeEdit();
        },
      },
    );
  };

  const confirmDelete = () => {
    if (!toDelete) return;
    remove.mutate(toDelete.id, {
      onSuccess: () => {
        notifySuccess("Intervention supprimée");
        closeDelete();
      },
    });
  };

  const submit = (input: InterventionInput) =>
    add.mutate(input, {
      onSuccess: () => {
        notifySuccess("Intervention enregistrée");
        close();
      },
    });

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
          <Typography variant="h6">Carnet d'entretien</Typography>
          <Stack flexGrow={1} />
          {status === "Retired" ? (
            <Typography variant="body2" color="text.secondary">
              Saisie indisponible : véhicule sorti du parc
            </Typography>
          ) : (
            <Button startIcon={<BuildIcon />} onClick={() => setOpen(true)}>
              Ajouter une intervention
            </Button>
          )}
        </Stack>
        {interventions.isError && <Alert severity="error">{interventionErrorMessage(interventions.error)}</Alert>}
        {interventions.data?.length === 0 && <Typography color="text.secondary">Aucune intervention.</Typography>}
        {interventions.data && interventions.data.length > 0 && (
          <InterventionList
            interventions={interventions.data}
            onDelete={status === "Retired" ? undefined : setToDelete}
            onEditAmount={setToEdit}
          />
        )}
      </Stack>
      {toEdit && (
        <EditAmountDialog
          intervention={toEdit}
          submitting={changeAmount.isPending}
          error={changeAmount.isError ? interventionErrorMessage(changeAmount.error) : null}
          onSubmit={submitAmount}
          onClose={closeEdit}
        />
      )}
      {toDelete && (
        <DeleteInterventionDialog
          intervention={toDelete}
          submitting={remove.isPending}
          error={remove.isError ? interventionErrorMessage(remove.error) : null}
          onConfirm={confirmDelete}
          onClose={closeDelete}
        />
      )}
      {open && (
        <AddInterventionDialog
          deadlines={(deadlines.data ?? []).filter((d) => !d.isClosed)}
          currentMileage={currentMileage}
          submitting={add.isPending}
          error={flow.message}
          reasonRequired={flow.reasonRequired}
          confirmationRequired={flow.confirmationRequired}
          onSubmit={submit}
          onConfirmationInvalidated={() => add.reset()}
          onClose={close}
        />
      )}
    </Paper>
  );
}
