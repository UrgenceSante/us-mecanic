import { Alert, Link as MuiLink, Paper, Stack, Typography } from "@mui/material";
import { Link } from "react-router";
import LogoLoader from "../../../components/Utils/LogoLoader";
import { useProblemReferences } from "../../problem-references/api/problemReferenceQueries";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { useVehicleProblems } from "../api/problemQueries";
import { problemStatusLabels } from "../types/labels";

/** Vue « Problèmes » de la fiche véhicule, dans l'ordre renvoyé par l'API (plus récent d'abord). */
export default function VehicleProblems({ vehicleId }: { vehicleId: string }) {
  const problems = useVehicleProblems(vehicleId);
  const natures = useProblemReferences("nature", true);
  const natureName = (id: string) => natures.data?.find((n) => n.id === id)?.name;

  return (
    <Paper sx={{ padding: 2 }}>
      <Typography variant="h6" marginBottom={1}>
        Problèmes
      </Typography>
      {problems.isLoading && <LogoLoader />}
      {problems.isError && <Alert severity="error">Impossible de charger les problèmes.</Alert>}
      {problems.data && problems.data.length === 0 && (
        <Typography color="text.secondary">Aucun problème enregistré pour ce véhicule.</Typography>
      )}
      {problems.data && problems.data.length > 0 && (
        <Stack
          component="ul"
          gap={1}
          sx={{ listStyle: "none", margin: 0, padding: 0 }}
          aria-label="Problèmes du véhicule"
        >
          {problems.data.map((p) => (
            <Stack component="li" key={p.id} direction="row" alignItems="center" gap={2} flexWrap="wrap">
              <Typography variant="body2">{formatDateOnly(p.date)}</Typography>
              <Typography variant="body2" minWidth={80}>
                {problemStatusLabels[p.status]}
              </Typography>
              <MuiLink component={Link} to={`/problems/${p.id}`} flexGrow={1}>
                {p.description}
              </MuiLink>
              <Typography variant="body2" color="text.secondary">
                {p.author}
              </Typography>
              {p.analysis && (
                <Typography variant="body2" color="text.secondary">
                  {[natureName(p.analysis.natureId), p.analysis.immobilized ? "Immobilisant" : null, p.analysis.text]
                    .filter(Boolean)
                    .join(" · ")}
                </Typography>
              )}
            </Stack>
          ))}
        </Stack>
      )}
    </Paper>
  );
}
