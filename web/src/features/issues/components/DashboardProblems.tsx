import { Alert, Button, Stack, Typography } from "@mui/material";
import { useState } from "react";
import LogoLoader from "../../../components/Utils/LogoLoader";
import { useFleetProblems } from "../api/problemQueries";
import { emptyFleetProblemFilter, type FleetProblemFilter } from "../types/problem";
import FleetProblemFilters from "./FleetProblemFilters";
import FleetProblemTable from "./FleetProblemTable";

/** Vue « Problèmes » du tableau de bord. */
export default function DashboardProblems() {
  const [filter, setFilter] = useState<FleetProblemFilter>(emptyFleetProblemFilter);
  const problems = useFleetProblems(filter);
  const isFiltered = (Object.keys(emptyFleetProblemFilter) as (keyof FleetProblemFilter)[]).some(
    (k) => filter[k] !== emptyFleetProblemFilter[k],
  );

  return (
    <Stack gap={2}>
      <FleetProblemFilters value={filter} onChange={setFilter} />
      {problems.isLoading && <LogoLoader />}
      {problems.isError && <Alert severity="error">Impossible de charger les problèmes.</Alert>}
      {problems.data && problems.data.length === 0 && (
        isFiltered ? (
          <Stack alignItems="flex-start" gap={1}>
            <Typography>Aucun problème ne correspond aux filtres</Typography>
            <Button onClick={() => setFilter(emptyFleetProblemFilter)}>Réinitialiser les filtres</Button>
          </Stack>
        ) : (
          <Typography>Aucun problème</Typography>
        )
      )}
      {problems.data && problems.data.length > 0 && <FleetProblemTable problems={problems.data} />}
    </Stack>
  );
}
