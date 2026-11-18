import { Link as MuiLink, Typography } from "@mui/material";
import type { GridColDef } from "@mui/x-data-grid";
import { Link } from "react-router";
import UrgencyBadge from "../../../shared/components/UrgencyBadge";
import type { Vehicle } from "../types/vehicle";
import { formatDateOnly } from "./vehicleForm";
import VehicleStatusChip from "./VehicleStatusChip";
import { currentUserLabel } from "./vehicleUserLabel";

/** Sous `xl` (1536 px) : colonnes les moins utiles masquées pour tenir sans défilement horizontal à 1440 px. */
export const desktopHiddenColumns = {
  internalNumber: false,
  firstRegistrationDate: false,
};

/** Sous `md` (tablette) : colonnes secondaires masquées pour laisser la place au statut et aux alertes. */
export const tabletHiddenColumns = {
  ...desktopHiddenColumns,
  ownerName: false,
  currentUserName: false,
  brand: false,
  model: false,
};

export const vehicleColumns: GridColDef<Vehicle>[] = [
  {
    field: "licensePlate",
    headerName: "Immatriculation",
    width: 130,
    // Vrai lien : seul moyen d'ouvrir la fiche au clavier (le clic sur la ligne reste disponible).
    renderCell: (params) => (
      <MuiLink component={Link} to={`/vehicles/${params.row.id}`} onClick={(e) => e.stopPropagation()}>
        {params.value}
      </MuiLink>
    ),
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
    width: 170,
    sortable: false,
    renderCell: (params) => <UrgencyBadge alerts={params.row.alerts} />,
  },
  { field: "internalNumber", headerName: "N° interne", width: 100 },
  { field: "typeName", headerName: "Type", width: 110 },
  {
    field: "ownerName",
    headerName: "Propriétaire",
    width: 150,
    valueGetter: (_value, row) => row.owner?.name ?? "À renseigner",
    renderCell: (params) =>
      params.row.owner ? (
        params.value
      ) : (
        <Typography variant="body2" color="warning.main">
          À renseigner
        </Typography>
      ),
  },
  {
    field: "currentUserName",
    headerName: "Utilisateur actuel",
    width: 170,
    valueGetter: (_value, row) => currentUserLabel(row),
    renderCell: (params) =>
      params.row.currentUser.company ? (
        params.value
      ) : (
        <Typography variant="body2" color="text.secondary">
          {params.value}
        </Typography>
      ),
  },
  { field: "brand", headerName: "Marque", flex: 1, minWidth: 70 },
  { field: "model", headerName: "Modèle", flex: 1, minWidth: 70 },
  {
    field: "firstRegistrationDate",
    headerName: "Mise en circulation",
    width: 140,
    valueFormatter: (value: string | null) => formatDateOnly(value),
  },
  {
    field: "currentMileage",
    headerName: "Km actuel",
    type: "number",
    width: 120,
    valueFormatter: (value: number | null) => (value == null ? "" : `${value.toLocaleString("fr-FR")} km`),
  },
];
