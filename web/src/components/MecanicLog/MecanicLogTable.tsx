import { Paper, Typography } from "@mui/material";
import {
  DataGrid,
  GridActionsCellItem,
  type GridColDef,
} from "@mui/x-data-grid";
import { frFR } from "@mui/x-data-grid/locales";
import SettingsIcon from "@mui/icons-material/Settings";
import useGetMecanicLogs from "../../hooks/mecanicLog/useGetMecanicLogs";
import { useNavigate } from "react-router";
import DateDisplayer from "../Utils/DateDisplayer";
import LogoLoader from "../Utils/LogoLoader";
import ErrorHandler from "../Utils/Error/ErrorHandler";
import type { IMecanicLog } from "./IMecanicLog";

export default function MecanicLogTable() {
  const request = useGetMecanicLogs();
  const navigate = useNavigate();

  const handleNavigate = (log: IMecanicLog) =>
    navigate(`/analyse/${log.LogId}`);

  const columns: GridColDef[] = [
    { field: "LogId", headerName: "LogId" },
    { field: "Immatriculation", headerName: "Immat" },
    { field: "ReportState", headerName: "Statut", minWidth: 120 },
    {
      field: "LogDate",
      headerName: "Date",
      renderCell: (params) => <DateDisplayer value={params.value} />,
    },
    {
      field: "Report",
      headerName: "Constat",
      flex: 1,
    },
    {
      field: "actions",
      type: "actions", // très important !
      headerName: "Actions",
      width: 100,
      getActions: (params) => [
        <GridActionsCellItem
          icon={<SettingsIcon color="action" />}
          label="Voir"
          color="primary"
          onClick={() => handleNavigate(params.row)}
          showInMenu={false}
        />,
      ],
    },
  ];

  if (request.isLoading) return <LogoLoader />;
  if (request.isError) return <ErrorHandler error={request.error} />;

  return (
    <>
      <Typography sx={{ marginY: 2 }} variant="h5">
        Liste des logs
      </Typography>
      <Paper sx={{ height: "100%" }}>
        <DataGrid
          getRowId={(row) => row.LogId}
          sx={{ border: "none", boxShadow: "none" }}
          rows={request.data}
          columns={columns}
          showToolbar
          localeText={frFR.components.MuiDataGrid.defaultProps.localeText}
        />
      </Paper>
    </>
  );
}
