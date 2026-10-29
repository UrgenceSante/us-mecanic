import { Link as MuiLink, Table, TableBody, TableCell, TableHead, TableRow } from "@mui/material";
import { Link } from "react-router";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { problemStatusLabels } from "../types/labels";
import type { FleetProblem } from "../types/problem";
import BlockingLevelChip from "./BlockingLevelChip";

/** Les lignes sont affichées dans l'ordre renvoyé par l'API (du plus au moins bloquant) : aucun tri ici. */
export default function FleetProblemTable({ problems }: { problems: FleetProblem[] }) {
  return (
    <Table size="small" aria-label="Problèmes du parc">
      <TableHead>
        <TableRow>
          <TableCell>Blocage</TableCell>
          <TableCell>Véhicule</TableCell>
          <TableCell>Date</TableCell>
          <TableCell>Description</TableCell>
          <TableCell>Statut</TableCell>
          <TableCell>Nature</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {problems.map(({ problem, vehicle, nature, blockingLevel }) => (
          <TableRow key={problem.id} hover>
            <TableCell>
              <BlockingLevelChip level={blockingLevel} />
            </TableCell>
            <TableCell>
              <MuiLink component={Link} to={`/vehicles/${vehicle.id}`}>
                {vehicle.licensePlate}
              </MuiLink>
            </TableCell>
            <TableCell>{formatDateOnly(problem.date)}</TableCell>
            <TableCell>
              <MuiLink component={Link} to={`/problems/${problem.id}`}>
                {problem.description}
              </MuiLink>
            </TableCell>
            <TableCell>{problemStatusLabels[problem.status]}</TableCell>
            <TableCell>{nature?.name ?? "-"}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
