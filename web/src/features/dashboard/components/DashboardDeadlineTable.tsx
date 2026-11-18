import ActionsHeaderCell from "../../../shared/components/ActionsHeaderCell";
import TaskAltIcon from "@mui/icons-material/TaskAlt";
import { IconButton, Link as MuiLink, Table, TableBody, TableCell, TableHead, TableRow, Tooltip } from "@mui/material";
import { Link } from "react-router";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import DeadlineGapText from "../../vehicle-deadlines/components/DeadlineGapText";
import DeadlineStatusChip from "../../vehicle-deadlines/components/DeadlineStatusChip";
import useBelow from "../../../shared/hooks/useBelow";
import { formatKm, internalNumberSuffix } from "../types/format";
import type { DashboardDeadline } from "../types/dashboard";
import DashboardDeadlineCards from "./DashboardDeadlineCards";

interface DashboardDeadlineTableProps {
  deadlines: DashboardDeadline[];
  onComplete: (deadline: DashboardDeadline) => void;
}

export default function DashboardDeadlineTable({ deadlines, onComplete }: DashboardDeadlineTableProps) {
  const compact = useBelow("sm");
  if (compact) return <DashboardDeadlineCards deadlines={deadlines} onComplete={onComplete} />;
  return (
    <Table size="small" aria-label="Échéances proches ou dépassées">
      <TableHead>
        <TableRow>
          <TableCell>Véhicule</TableCell>
          <TableCell>Type d'échéance</TableCell>
          <TableCell>Statut</TableCell>
          <TableCell>Écart</TableCell>
          <TableCell>Échéance (date)</TableCell>
          <TableCell>Échéance (km)</TableCell>
          <TableCell>Km actuel</TableCell>
          <ActionsHeaderCell />
        </TableRow>
      </TableHead>
      <TableBody>
        {deadlines.map((d) => (
          <TableRow key={d.id} hover>
            <TableCell>
              <MuiLink component={Link} to={`/vehicles/${d.vehicle.id}`}>
                {d.vehicle.licensePlate}
              </MuiLink>
              {internalNumberSuffix(d.vehicle)}
            </TableCell>
            <TableCell>{d.deadlineType.name}</TableCell>
            <TableCell>
              <DeadlineStatusChip status={d.status} isClosed={false} />
            </TableCell>
            <TableCell>
              <DeadlineGapText dueDate={d.dueDate} dueKm={d.dueKm} currentKm={d.currentKm} status={d.status} />
            </TableCell>
            <TableCell>{formatDateOnly(d.dueDate)}</TableCell>
            <TableCell>{formatKm(d.dueKm)}</TableCell>
            <TableCell>{formatKm(d.currentKm)}</TableCell>
            <TableCell align="right">
              <Tooltip title="Marquer réalisée">
                <IconButton
                  aria-label={`Marquer réalisée : ${d.deadlineType.name} ${d.vehicle.licensePlate}`}
                  onClick={() => onComplete(d)}
                >
                  <TaskAltIcon />
                </IconButton>
              </Tooltip>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
