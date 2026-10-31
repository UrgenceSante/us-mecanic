import { Button, Paper, Stack, Typography } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import KeyboardArrowLeftIcon from "@mui/icons-material/KeyboardArrowLeft";
import { useState } from "react";
import { Link, useParams } from "react-router";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import LogoLoader from "../../../components/Utils/LogoLoader";
import PropertyDisplay from "../../../components/Utils/PropertyDisplay";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { problemMessage } from "../../../shared/api/problemMessage";
import { useChangeVehicleStatus, useUpdateVehicle, useVehicle } from "../api/vehicleQueries";
import VehicleDeadlines from "../../vehicle-deadlines/components/VehicleDeadlines";
import VehicleMileage from "../../mileage/components/VehicleMileage";
import VehicleMaintenanceLog from "../../maintenance/components/VehicleMaintenanceLog";
import VehicleProblems from "../../issues/components/VehicleProblems";
import VehicleExpenses from "../../costs/components/VehicleExpenses";
import VehicleLoans from "../../costs/components/VehicleLoans";
import VehicleFormDialog from "../components/VehicleFormDialog";
import VehicleStatusActions from "../components/VehicleStatusActions";
import VehicleStatusChip from "../components/VehicleStatusChip";
import { formatDateOnly, toDetailsInput, toFormValues, type VehicleFormValues } from "../components/vehicleForm";
import { vehicleStatusLabels, type VehicleStatus } from "../types/vehicle";

export default function VehicleDetailPage() {
  const { vehicleId = "" } = useParams();
  const { notifySuccess, notifyError } = useNotifSnack();
  const [editing, setEditing] = useState(false);

  const vehicle = useVehicle(vehicleId);
  const update = useUpdateVehicle(vehicleId);
  const changeStatus = useChangeVehicleStatus(vehicleId);

  if (vehicle.isLoading) return <LogoLoader />;
  if (vehicle.isError || !vehicle.data)
    return <ErrorHandler error={vehicle.error} custom404Render={<Typography marginY={2}>Véhicule introuvable.</Typography>} />;

  const data = vehicle.data;

  const handleUpdate = (values: VehicleFormValues) =>
    update.mutate(toDetailsInput(values), {
      onSuccess: () => {
        notifySuccess("Véhicule modifié");
        setEditing(false);
      },
    });

  const handleStatus = (status: VehicleStatus) =>
    changeStatus.mutate(status, {
      onSuccess: () => notifySuccess(`Statut : ${vehicleStatusLabels[status]}`),
      onError: (error) => notifyError(problemMessage(error)),
    });

  const closeDialog = () => {
    setEditing(false);
    update.reset();
  };

  return (
    <Stack gap={2} marginY={2}>
      <div>
        <Button component={Link} to="/vehicles" startIcon={<KeyboardArrowLeftIcon />}>
          Parc
        </Button>
      </div>
      <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
        <Typography variant="h5">{data.licensePlate}</Typography>
        <VehicleStatusChip status={data.status} />
        <Stack flexGrow={1} />
        {data.status !== "Retired" && (
          <Button startIcon={<EditIcon />} onClick={() => setEditing(true)}>
            Modifier
          </Button>
        )}
      </Stack>

      <Paper sx={{ padding: 2 }}>
        <Stack gap={1}>
          <PropertyDisplay title="Marque" content={data.brand ?? "-"} />
          <PropertyDisplay title="Modèle" content={data.model ?? "-"} />
          <PropertyDisplay title="N° interne / indicatif" content={data.internalNumber ?? "-"} />
          <PropertyDisplay title="Mise en circulation" content={formatDateOnly(data.firstRegistrationDate)} />
          <PropertyDisplay title="Type" content={data.typeName ?? "-"} />
          <PropertyDisplay title="VIN" content={data.vin ?? "-"} />
        </Stack>
      </Paper>

      <VehicleMileage
        vehicleId={data.id}
        status={data.status}
        currentMileage={data.currentMileage}
        currentMileageDate={data.currentMileageDate}
      />

      <VehicleDeadlines vehicleId={data.id} status={data.status} currentMileage={data.currentMileage} />

      <VehicleMaintenanceLog vehicleId={data.id} status={data.status} currentMileage={data.currentMileage} />

      <VehicleStatusActions status={data.status} disabled={changeStatus.isPending} onChange={handleStatus} />

      <VehicleProblems vehicleId={data.id} />

      <div>
        <Button component={Link} to={`/costs?vehicleId=${data.id}`} size="small">
          Voir les coûts
        </Button>
      </div>

      <VehicleExpenses vehicleId={data.id} status={data.status} />

      <VehicleLoans vehicleId={data.id} status={data.status} />

      {editing && (
        <VehicleFormDialog
          open
          initialValues={toFormValues(data)}
          currentType={data.typeId && data.typeName ? { id: data.typeId, name: data.typeName } : null}
          submitting={update.isPending}
          error={update.isError ? problemMessage(update.error) : null}
          onSubmit={handleUpdate}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
