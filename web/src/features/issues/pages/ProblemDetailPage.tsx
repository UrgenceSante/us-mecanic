import KeyboardArrowLeftIcon from "@mui/icons-material/KeyboardArrowLeft";
import { Button, Link as MuiLink, Paper, Stack, Typography } from "@mui/material";
import { Link, useParams } from "react-router";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import LogoLoader from "../../../components/Utils/LogoLoader";
import PropertyDisplay from "../../../components/Utils/PropertyDisplay";
import { useVehicle } from "../../vehicles/api/vehicleQueries";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { useProblem } from "../api/problemQueries";
import ProblemActions from "../components/ProblemActions";
import ProblemAnalysisForm from "../components/ProblemAnalysisForm";
import ProblemAnalysisView from "../components/ProblemAnalysisView";
import { problemStatusLabels } from "../types/labels";

export default function ProblemDetailPage() {
  const { problemId = "" } = useParams();
  const problem = useProblem(problemId);
  const vehicle = useVehicle(problem.data?.vehicleId ?? "");

  if (problem.isLoading) return <LogoLoader />;
  if (problem.isError || !problem.data)
    return <ErrorHandler error={problem.error} custom404Render={<Typography marginY={2}>Problème introuvable.</Typography>} />;

  const data = problem.data;

  return (
    <Stack gap={2} marginY={2}>
      <div>
        <Button component={Link} to="/" startIcon={<KeyboardArrowLeftIcon />}>
          Tableau de bord
        </Button>
      </div>
      <Paper sx={{ padding: 2 }}>
        <Typography variant="h5" marginBottom={1}>
          Problème
        </Typography>
        <PropertyDisplay
          title="Véhicule"
          content={
            <MuiLink component={Link} to={`/vehicles/${data.vehicleId}`}>
              {vehicle.data?.licensePlate ?? "Voir le véhicule"}
            </MuiLink>
          }
        />
        <PropertyDisplay title="Date" content={formatDateOnly(data.date)} />
        <PropertyDisplay title="Description" content={data.description} />
        <PropertyDisplay title="Auteur" content={data.author} />
        <PropertyDisplay title="Statut" content={problemStatusLabels[data.status]} />
      </Paper>
      {data.analysis ? (
        <ProblemAnalysisView analysis={data.analysis} />
      ) : (
        <ProblemAnalysisForm problemId={data.id} vehicleId={data.vehicleId} />
      )}
      <ProblemActions problem={data} />
    </Stack>
  );
}
