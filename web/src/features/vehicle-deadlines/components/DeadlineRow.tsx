import { Button, ListItem, ListItemText, Stack } from "@mui/material";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { VehicleDeadline } from "../types/vehicleDeadline";
import DeadlineStatusChip from "./DeadlineStatusChip";

const formatKm = (km: number) => `${km.toLocaleString("fr-FR")} km`;

function summary(prefix: string, empty: string, date: string | null, km: number | null): string {
  const parts: string[] = [];
  if (date) parts.push(formatDateOnly(date));
  if (km !== null) parts.push(formatKm(km));
  return parts.length > 0 ? `${prefix} : ${parts.join(" / ")}` : empty;
}

interface DeadlineRowProps {
  deadline: VehicleDeadline;
  /** Action « Marquer réalisée » disponible (échéance en cours, véhicule dans le parc). */
  canComplete: boolean;
  onComplete: (deadline: VehicleDeadline) => void;
}

export default function DeadlineRow({ deadline: d, canComplete, onComplete }: DeadlineRowProps) {
  const next = summary("Prochaine", "Aucune prochaine échéance", d.dueDate, d.dueKm);
  const last = summary("Dernière réalisation", "Jamais réalisée", d.lastDoneDate, d.lastDoneKm);
  return (
    <ListItem
      disableGutters
      secondaryAction={
        canComplete && !d.isClosed ? (
          <Button size="small" aria-label={`Marquer réalisée : ${d.typeName ?? ""}`} onClick={() => onComplete(d)}>
            Marquer réalisée
          </Button>
        ) : undefined
      }
    >
      <ListItemText
        primary={
          <Stack direction="row" alignItems="center" gap={1}>
            <span>{d.typeName ?? "Type inconnu"}</span>
            <DeadlineStatusChip status={d.status} isClosed={d.isClosed} />
          </Stack>
        }
        secondary={`${next} | ${last}`}
        slotProps={{ primary: { component: "div" } }}
        sx={{ paddingRight: 18 }}
      />
    </ListItem>
  );
}
