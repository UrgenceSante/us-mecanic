import { Button, Paper, Stack, Typography } from "@mui/material";
import { Link } from "react-router";
import DateDisplayer from "../../../components/Utils/DateDisplayer";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import LogoLoader from "../../../components/Utils/LogoLoader";
import { useVehicleProblems } from "../api/vehicleProblemsQueries";

interface Props {
  licensePlate: string;
}

/** Vue « Problèmes » de la fiche véhicule (logs de l'ancienne API, transitoire). */
export default function VehicleProblems({ licensePlate }: Props) {
  const problems = useVehicleProblems(licensePlate);

  return (
    <Paper sx={{ padding: 2 }}>
      <Typography variant="h6" marginBottom={1}>
        Problèmes
      </Typography>
      {problems.isLoading && <LogoLoader />}
      {problems.isError && <ErrorHandler error={problems.error} />}
      {problems.data && problems.data.length === 0 && (
        <Typography color="text.secondary">Aucun problème enregistré pour ce véhicule.</Typography>
      )}
      {problems.data && problems.data.length > 0 && (
        <Stack gap={1}>
          {problems.data.map((log) => (
            <Stack key={log.LogId} direction="row" alignItems="center" gap={2} flexWrap="wrap">
              <DateDisplayer value={log.LogDate} />
              <Typography variant="body2" minWidth={100}>
                {log.ReportState}
              </Typography>
              <Typography flexGrow={1}>{log.Report}</Typography>
              <Button component={Link} to={`/analyse/${log.LogId}`} size="small">
                Voir l'analyse
              </Button>
            </Stack>
          ))}
        </Stack>
      )}
    </Paper>
  );
}
