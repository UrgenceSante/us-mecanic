import { Chip } from "@mui/material";
import { deadlineStatusColors, deadlineStatusLabels, type DeadlineStatus } from "../types/vehicleDeadline";

/** Statut par couleur et libellé ; « Close » pour une échéance close, « Inconnu » si l'API n'a pas pu le calculer. */
export default function DeadlineStatusChip({ status, isClosed }: { status: DeadlineStatus | null; isClosed: boolean }) {
  if (isClosed) return <Chip size="small" label="Close" />;
  if (status === null) return <Chip size="small" label="Inconnu" />;
  return <Chip size="small" color={deadlineStatusColors[status]} label={deadlineStatusLabels[status]} />;
}
