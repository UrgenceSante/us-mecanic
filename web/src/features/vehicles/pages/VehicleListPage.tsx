import PageTitle from "../../../shared/components/PageTitle";
import { Button, Paper, Stack } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { frFR } from "@mui/x-data-grid/locales";
import AddIcon from "@mui/icons-material/Add";
import UploadFileIcon from "@mui/icons-material/UploadFile";
import { useState } from "react";
import { Link, useNavigate } from "react-router";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { useRegisterVehicle, useVehicles } from "../api/vehicleQueries";
import useVehicleListFilters from "../hooks/useVehicleListFilters";
import VehicleListFilters from "../components/VehicleListFilters";
import useBelow from "../../../shared/hooks/useBelow";
import VehicleCards from "../components/VehicleCards";
import VehicleFormDialog from "../components/VehicleFormDialog";
import NoVehicleFound from "../components/NoVehicleFound";
import { desktopHiddenColumns, tabletHiddenColumns, vehicleColumns } from "../components/vehicleColumns";
import { toRegisterCommand, type VehicleFormValues } from "../components/vehicleForm";

export default function VehicleListPage() {
  const navigate = useNavigate();
  const { notifySuccess } = useNotifSnack();
  const [creating, setCreating] = useState(false);
  const compact = useBelow("sm");
  const tablet = useBelow("md");
  const desktopNarrow = useBelow("xl");
  const { values, setters, filter, filtered, reset: resetFilters } = useVehicleListFilters();
  const vehicles = useVehicles(filter);
  const register = useRegisterVehicle();

  const handleCreate = (values: VehicleFormValues) =>
    register.mutate(toRegisterCommand(values), {
      onSuccess: (vehicle) => {
        notifySuccess(`Véhicule ${vehicle.licensePlate} ajouté`);
        setCreating(false);
      },
    });

  const closeDialog = () => {
    setCreating(false);
    register.reset();
  };

  return (
    <Stack gap={2} marginY={2}>
      <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
        <PageTitle flexGrow={1}>Parc de véhicules</PageTitle>
        <Button component={Link} to="/vehicles/import" startIcon={<UploadFileIcon />}>
          Importer
        </Button>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setCreating(true)}>
          Ajouter
        </Button>
      </Stack>

      <VehicleListFilters values={values} setters={setters} />

      {vehicles.isError ? (
        <ErrorHandler error={vehicles.error} />
      ) : compact ? (
        <VehicleCards vehicles={vehicles.data ?? []} loading={vehicles.isLoading} filtered={filtered} onReset={resetFilters} />
      ) : (
        <Paper>
          <DataGrid
            rows={vehicles.data ?? []}
            columns={vehicleColumns}
            columnVisibilityModel={tablet ? tabletHiddenColumns : desktopNarrow ? desktopHiddenColumns : {}}
            slots={{
              noRowsOverlay: () => <NoVehicleFound filtered={filtered} onReset={resetFilters} />,
            }}
            loading={vehicles.isLoading}
            onRowClick={(params) => navigate(`/vehicles/${params.id}`)}
            localeText={frFR.components.MuiDataGrid.defaultProps.localeText}
            initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
            pageSizeOptions={[25, 50, 100]}
            disableRowSelectionOnClick
            autoHeight
            sx={{ "& .MuiDataGrid-row": { cursor: "pointer" } }}
          />
        </Paper>
      )}

      {creating && (
        <VehicleFormDialog
          open
          submitting={register.isPending}
          error={register.isError ? register.error : null}
          onSubmit={handleCreate}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
