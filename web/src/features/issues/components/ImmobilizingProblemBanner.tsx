import { Alert, Link as MuiLink } from "@mui/material";
import { Link } from "react-router";
import { useVehicleProblems } from "../api/problemQueries";
import type { Problem } from "../types/problem";

/** Premier problème immobilisant encore ouvert (analysé comme immobilisant et non résolu). */
function findImmobilizingProblem(problems: Problem[] | undefined): Problem | undefined {
  return problems?.find((p) => p.status !== "Resolved" && p.analysis?.immobilized === true);
}

/** Bandeau d'alerte de la fiche véhicule. Réutilise la requête des problèmes déjà chargée par la fiche. */
export default function ImmobilizingProblemBanner({ vehicleId }: { vehicleId: string }) {
  const problems = useVehicleProblems(vehicleId);
  const problem = findImmobilizingProblem(problems.data);
  if (!problem) return null;

  return (
    <Alert severity="error">
      Problème immobilisant en cours :{" "}
      <MuiLink component={Link} to={`/problems/${problem.id}`} color="inherit" fontWeight="bold">
        {problem.description}
      </MuiLink>
    </Alert>
  );
}
