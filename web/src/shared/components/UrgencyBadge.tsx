import BlockIcon from "@mui/icons-material/Block";
import ErrorIcon from "@mui/icons-material/Error";
import { Stack } from "@mui/material";
import StatusChip from "./StatusChip";

export interface UrgencyAlerts {
  overdueDeadlines: number;
  immobilizingProblem: boolean;
}

const overdueLabel = (n: number) => `${n} échéance${n > 1 ? "s" : ""} dépassée${n > 1 ? "s" : ""}`;

/** Alertes d'un véhicule (problème immobilisant, échéances dépassées) en puces avec icône et texte. */
export default function UrgencyBadge({ alerts }: { alerts: UrgencyAlerts | null | undefined }) {
  if (!alerts || (!alerts.immobilizingProblem && alerts.overdueDeadlines === 0)) return null;
  return (
    <Stack direction="row" gap={0.5} flexWrap="wrap" alignItems="center">
      {alerts.immobilizingProblem && <StatusChip tone="error" icon={<BlockIcon />} label="Problème immobilisant" category="Alerte" />}
      {alerts.overdueDeadlines > 0 && <StatusChip tone="error" icon={<ErrorIcon />} label={overdueLabel(alerts.overdueDeadlines)} category="Alerte" />}
    </Stack>
  );
}
