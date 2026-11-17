import { Link as MuiLink, Stack, Typography } from "@mui/material";
import { Link } from "react-router";
import ListCard from "../../../shared/components/ListCard";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { problemStatusLabels } from "../types/labels";
import type { FleetProblem } from "../types/problem";
import BlockingLevelChip from "./BlockingLevelChip";

/** Rendu mobile : un problème par carte, dans l'ordre renvoyé par l'API. */
export default function FleetProblemCards({ problems }: { problems: FleetProblem[] }) {
  return (
    <Stack gap={1} role="list" aria-label="Problèmes du parc">
      {problems.map(({ problem, vehicle, nature, blockingLevel }) => (
        <ListCard key={problem.id}>
          <Stack gap={0.5}>
            <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
              <BlockingLevelChip level={blockingLevel} />
              <MuiLink component={Link} to={`/vehicles/${vehicle.id}`} fontWeight="bold">
                {vehicle.licensePlate}
              </MuiLink>
            </Stack>
            <MuiLink component={Link} to={`/problems/${problem.id}`} sx={{ overflowWrap: "anywhere" }}>
              {problem.description}
            </MuiLink>
            <Typography variant="body2" color="text.secondary">
              {formatDateOnly(problem.date)} · {problemStatusLabels[problem.status]} · {nature?.name ?? "-"}
            </Typography>
          </Stack>
        </ListCard>
      ))}
    </Stack>
  );
}
