import PageTitle from "../../../shared/components/PageTitle";
import { Button, Link as MuiLink, MenuItem, Paper, Stack, TextField } from "@mui/material";
import { DataGrid, type GridColDef } from "@mui/x-data-grid";
import { frFR } from "@mui/x-data-grid/locales";
import AddIcon from "@mui/icons-material/Add";
import UploadFileIcon from "@mui/icons-material/UploadFile";
import { useState } from "react";
import { Link, useNavigate } from "react-router";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { problemMessage } from "../../../shared/api/problemMessage";
import { useRegisterVehicle, useVehicles } from "../api/vehicleQueries";
import VehicleTypeSelect from "../../vehicle-types/components/VehicleTypeSelect";
import UrgencyBadge from "../../../shared/components/UrgencyBadge";
import useBelow from "../../../shared/hooks/useBelow";
import VehicleCards from "../components/VehicleCards";
import VehicleFormDialog from "../components/VehicleFormDialog";
import VehicleStatusChip from "../components/VehicleStatusChip";
import { formatDateOnly, toRegisterCommand, type VehicleFormValues } from "../components/vehicleForm";
import { vehicleStatusLabels, type Vehicle, type VehicleStatus } from "../types/vehicle";

const columns: GridColDef<Vehicle>[] = [
  {
    field: "licensePlate",
    headerName: "Immatriculation",
    width: 140,
    // Vrai lien : seul moyen d'ouvrir la fiche au clavier (le clic sur la ligne reste disponible).
    renderCell: (params) => (
      <MuiLink component={Link} to={`/vehicles/${params.row.id}`} onClick={(e) => e.stopPropagation()}>
        {params.value}
      </MuiLink>
    ),
  },
  { field: "internalNumber", headerName: "N° interne", width: 120 },
  { field: "typeName", headerName: "Type", width: 140 },
  { field: "brand", headerName: "Marque", flex: 1, minWidth: 120 },
  { field: "model", headerName: "Modèle", flex: 1, minWidth: 120 },
  {
    field: "firstRegistrationDate",
    headerName: "Mise en circulation",
    width: 160,
    valueFormatter: (value: string | null) => formatDateOnly(value),
  },
  {
    field: "currentMileage",
    headerName: "Km actuel",
    type: "number",
    width: 130,
    valueFormatter: (value: number | null) => (value == null ? "" : `${value.toLocaleString("fr-FR")} km`),
  },
  {
    field: "status",
    headerName: "Statut",
    width: 140,
    renderCell: (params) => <VehicleStatusChip status={params.value} />,
  },
  {
    field: "alerts",
    headerName: "Alertes",
    width: 280,
    sortable: false,
    renderCell: (params) => <UrgencyBadge alerts={params.row.alerts} />,
  },
];

export default function VehicleListPage() {
  const navigate = useNavigate();
  const { notifySuccess } = useNotifSnack();
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState<VehicleStatus | "">("");
  const [type, setType] = useState("");
  const [creating, setCreating] = useState(false);
  const compact = useBelow("sm");

  const vehicles = useVehicles({ search, status: status || undefined, type: type || undefined });
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

      <Stack direction={{ xs: "column", sm: "row" }} gap={2}>
        <TextField
          label="Rechercher"
          placeholder="Immatriculation, marque, modèle, n° interne"
          size="small"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ flex: 1 }}
        />
        <TextField
          select
          label="Statut"
          size="small"
          value={status}
          onChange={(e) => setStatus(e.target.value as VehicleStatus | "")}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">Tous</MenuItem>
          {Object.entries(vehicleStatusLabels).map(([value, label]) => (
            <MenuItem key={value} value={value}>
              {label}
            </MenuItem>
          ))}
        </TextField>
        <VehicleTypeSelect
          value={type}
          onChange={setType}
          emptyLabel="Tous"
          includeInactive
          size="small"
          minWidth={180}
        />
      </Stack>

      {vehicles.isError ? (
        <ErrorHandler error={vehicles.error} />
      ) : compact ? (
        <VehicleCards vehicles={vehicles.data ?? []} loading={vehicles.isLoading} />
      ) : (
        <Paper>
          <DataGrid
            rows={vehicles.data ?? []}
            columns={columns}
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
          error={register.isError ? problemMessage(register.error) : null}
          onSubmit={handleCreate}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
