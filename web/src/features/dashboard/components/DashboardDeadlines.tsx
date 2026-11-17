import { Alert, Stack, Typography } from "@mui/material";
import { useMemo, useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { completionFlow } from "../../vehicle-deadlines/api/completionFlow";
import { deadlineErrorMessage } from "../../vehicle-deadlines/api/vehicleDeadlineErrors";
import { useCompleteVehicleDeadline } from "../../vehicle-deadlines/api/vehicleDeadlineQueries";
import CompleteDeadlineDialog from "../../vehicle-deadlines/components/CompleteDeadlineDialog";
import type { DeadlineCompletionInput, VehicleDeadline } from "../../vehicle-deadlines/types/vehicleDeadline";
import { useDashboardDeadlines } from "../api/dashboardQueries";
import { emptyDashboardFilter, type DashboardDeadline, type DashboardFilter } from "../types/dashboard";
import DashboardDeadlineTable from "./DashboardDeadlineTable";
import DashboardFilters from "./DashboardFilters";
import { countDeadlines, sortDeadlines } from "./sortDeadlines";

/** Le dialogue de réalisation attend une échéance de véhicule : on reconstruit ce que la ligne en connaît. */
function toVehicleDeadline(d: DashboardDeadline): VehicleDeadline {
  return {
    id: d.id,
    vehicleId: d.vehicle.id,
    deadlineTypeId: d.deadlineType.id,
    typeName: d.deadlineType.name,
    intervalMonths: null,
    intervalKm: null,
    lastDoneDate: null,
    lastDoneKm: null,
    dueDate: d.dueDate,
    dueKm: d.dueKm,
    isClosed: false,
    status: d.status,
  };
}

export default function DashboardDeadlines() {
  const { notifySuccess } = useNotifSnack();
  const [filter, setFilter] = useState<DashboardFilter>(emptyDashboardFilter);
  const [completing, setCompleting] = useState<DashboardDeadline | null>(null);
  const deadlines = useDashboardDeadlines(filter);
  const complete = useCompleteVehicleDeadline(completing?.vehicle.id ?? "", completing?.id ?? "");
  const sorted = useMemo(() => sortDeadlines(deadlines.data ?? []), [deadlines.data]);
  const counts = countDeadlines(sorted);
  const isFiltered = (Object.keys(emptyDashboardFilter) as (keyof DashboardFilter)[]).some(
    (k) => filter[k] !== emptyDashboardFilter[k],
  );
  const flow = completionFlow(complete.isError ? complete.error : null);

  const close = () => {
    setCompleting(null);
    complete.reset();
  };
  const submit = (input: DeadlineCompletionInput) =>
    complete.mutate(input, {
      onSuccess: () => {
        notifySuccess("Échéance réalisée");
        close();
      },
    });

  return (
    <Stack gap={2}>
      <DashboardFilters value={filter} onChange={setFilter} />
      {deadlines.isError && <Alert severity="error">{deadlineErrorMessage(deadlines.error)}</Alert>}
      {deadlines.data && deadlines.data.length === 0 && (
        isFiltered ? (
          <Typography>Aucune échéance ne correspond aux filtres</Typography>
        ) : (
          <Alert severity="success">Rien d'urgent : toutes les échéances sont à jour</Alert>
        )
      )}
      {sorted.length > 0 && (
        <>
          <Typography variant="subtitle1">
            {counts.overdue} {counts.overdue > 1 ? "dépassées" : "dépassée"} · {counts.dueSoon}{" "}
            {counts.dueSoon > 1 ? "proches" : "proche"}
          </Typography>
          <DashboardDeadlineTable deadlines={sorted} onComplete={setCompleting} />
        </>
      )}
      {completing && (
        <CompleteDeadlineDialog
          deadline={toVehicleDeadline(completing)}
          currentMileage={completing.currentKm}
          submitting={complete.isPending}
          error={flow.message}
          reasonRequired={flow.reasonRequired}
          confirmationRequired={flow.confirmationRequired}
          onSubmit={submit}
          onConfirmationInvalidated={() => complete.reset()}
          onClose={close}
        />
      )}
    </Stack>
  );
}
