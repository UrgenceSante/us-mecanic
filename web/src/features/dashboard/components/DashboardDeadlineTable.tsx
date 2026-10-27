import { Button, Link as MuiLink, Table, TableBody, TableCell, TableHead, TableRow } from "@mui/material";
import { Link } from "react-router";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import DeadlineStatusChip from "../../vehicle-deadlines/components/DeadlineStatusChip";
import type { DashboardDeadline } from "../types/dashboard";

const formatKm = (km: number | null) => (km === null ? "-" : `${km.toLocaleString("fr-FR")} km`);

interface DashboardDeadlineTableProps {
  deadlines: DashboardDeadline[];
  onComplete: (deadline: DashboardDeadline) => void;
}

export default function DashboardDeadlineTable({ deadlines, onComplete }: DashboardDeadlineTableProps) {
  return (
    <Table size="small" aria-label="Échéances proches ou dépassées">
      <TableHead>
        <TableRow>
          <TableCell>Véhicule</TableCell>
          <TableCell>Type d'échéance</TableCell>
          <TableCell>Statut</TableCell>
          <TableCell>Échéance (date)</TableCell>
          <TableCell>Échéance (km)</TableCell>
          <TableCell>Km actuel</TableCell>
          <TableCell />
        </TableRow>
      </TableHead>
      <TableBody>
        {deadlines.map((d) => (
          <TableRow key={d.id} hover>
            <TableCell>
              <MuiLink component={Link} to={`/vehicles/${d.vehicle.id}`}>
                {d.vehicle.licensePlate}
              </MuiLink>
              {d.vehicle.internalNumber ? ` (${d.vehicle.internalNumber})` : ""}
            </TableCell>
            <TableCell>{d.deadlineType.name}</TableCell>
            <TableCell>
              <DeadlineStatusChip status={d.status} isClosed={false} />
            </TableCell>
            <TableCell>{formatDateOnly(d.dueDate)}</TableCell>
            <TableCell>{formatKm(d.dueKm)}</TableCell>
            <TableCell>{formatKm(d.currentKm)}</TableCell>
            <TableCell align="right">
              <Button
                size="small"
                aria-label={`Marquer réalisée : ${d.deadlineType.name} ${d.vehicle.licensePlate}`}
                onClick={() => onComplete(d)}
              >
                Marquer réalisée
              </Button>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
