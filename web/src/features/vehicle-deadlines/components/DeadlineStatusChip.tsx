import { Chip } from "@mui/material";
import { deadlineStatusColors, deadlineStatusLabels, type DeadlineStatus } from "../types/vehicleDeadline";

/** Statut par couleur et libellé ; « Close » quand le statut est nul. */
export default function DeadlineStatusChip({ status }: { status: DeadlineStatus | null }) {
  if (status === null) return <Chip size="small" label="Close" />;
  return <Chip size="small" color={deadlineStatusColors[status]} label={deadlineStatusLabels[status]} />;
}
