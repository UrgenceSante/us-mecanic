import { Button, Link as MuiLink, Stack, Typography } from "@mui/material";
import { Link } from "react-router";
import ListCard from "../../../shared/components/ListCard";
import DeadlineStatusChip from "../../vehicle-deadlines/components/DeadlineStatusChip";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { DashboardDeadline } from "../types/dashboard";

const formatKm = (km: number | null) => (km === null ? "-" : `${km.toLocaleString("fr-FR")} km`);

interface Props {
  deadlines: DashboardDeadline[];
  onComplete: (deadline: DashboardDeadline) => void;
}

/** Rendu mobile : une échéance par carte. */
export default function DashboardDeadlineCards({ deadlines, onComplete }: Props) {
  return (
    <Stack gap={1} aria-label="Échéances proches ou dépassées">
      {deadlines.map((d) => (
        <ListCard key={d.id}>
          <Stack gap={0.5}>
            <Typography sx={{ overflowWrap: "anywhere" }}>
              <MuiLink component={Link} to={`/vehicles/${d.vehicle.id}`} fontWeight="bold">
                {d.vehicle.licensePlate}
              </MuiLink>
              {d.vehicle.internalNumber ? ` (${d.vehicle.internalNumber})` : ""} · {d.deadlineType.name}
            </Typography>
            <Stack direction="row" gap={1} flexWrap="wrap">
              <DeadlineStatusChip status={d.status} isClosed={false} />
            </Stack>
            <Typography variant="body2" color="text.secondary">
              {formatDateOnly(d.dueDate)} · {formatKm(d.dueKm)} (actuel : {formatKm(d.currentKm)})
            </Typography>
            <Button
              fullWidth
              variant="outlined"
              aria-label={`Marquer réalisée : ${d.deadlineType.name} ${d.vehicle.licensePlate}`}
              onClick={() => onComplete(d)}
            >
              Marquer réalisée
            </Button>
          </Stack>
        </ListCard>
      ))}
    </Stack>
  );
}
