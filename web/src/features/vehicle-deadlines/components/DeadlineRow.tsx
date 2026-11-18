import { Button, ListItem, ListItemText, Stack } from "@mui/material";
import useBelow from "../../../shared/hooks/useBelow";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { VehicleDeadline } from "../types/vehicleDeadline";
import DeadlineGapText from "./DeadlineGapText";
import DeadlineStatusChip from "./DeadlineStatusChip";

const formatKm = (km: number) => `${km.toLocaleString("fr-FR")}\u00a0km`;

/** Résumé « Prochaine : 12/10/2026 / 50 000 km ». Le critère dépassé est dit par la ligne d'écart. */
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
  /** Km actuel du véhicule, pour l'écart kilométrique. */
  currentMileage: number | null;
  onComplete: (deadline: VehicleDeadline) => void;
}

export default function DeadlineRow({ deadline: d, canComplete, currentMileage, onComplete }: DeadlineRowProps) {
  const compact = useBelow("sm");
  const next = summary("Prochaine", "Aucune prochaine échéance", d.dueDate, d.dueKm);
  const last = summary("Dernière réalisation", "Jamais réalisée", d.lastDoneDate, d.lastDoneKm);
  const showAction = canComplete && !d.isClosed;
  const action = (
    <Button
      size="small"
      variant={compact ? "outlined" : "text"}
      fullWidth={compact}
      aria-label={`Marquer réalisée : ${d.typeName ?? ""}`}
      onClick={() => onComplete(d)}
    >
      Marquer réalisée
    </Button>
  );
  return (
    <ListItem
      disableGutters
      sx={compact ? { flexDirection: "column", alignItems: "stretch", gap: 1 } : undefined}
      secondaryAction={showAction && !compact ? action : undefined}
    >
      <ListItemText
        primary={
          <Stack direction="row" alignItems="center" gap={1}>
            <span>{d.typeName ?? "Type inconnu"}</span>
            <DeadlineStatusChip status={d.status} isClosed={d.isClosed} />
          </Stack>
        }
        secondary={
          <>
            {next}
            <br />
            <DeadlineGapText dueDate={d.dueDate} dueKm={d.dueKm} currentKm={currentMileage} status={d.status} />
            <br />
            {last}
          </>
        }
        slotProps={{ primary: { component: "div" } }}
        sx={{ paddingRight: compact ? 0 : 18, margin: 0 }}
      />
      {showAction && compact && action}
    </ListItem>
  );
}
