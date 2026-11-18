import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ErrorIcon from "@mui/icons-material/Error";
import HelpOutlineIcon from "@mui/icons-material/HelpOutline";
import LockIcon from "@mui/icons-material/Lock";
import ScheduleIcon from "@mui/icons-material/Schedule";
import type { ReactElement } from "react";
import StatusChip, { type StatusTone } from "../../../shared/components/StatusChip";
import { deadlineStatusLabels, type DeadlineStatus } from "../types/vehicleDeadline";

const deadlineStatusChips: Record<DeadlineStatus, { tone: StatusTone; icon: ReactElement }> = {
  UpToDate: { tone: "success", icon: <CheckCircleIcon /> },
  DueSoon: { tone: "warning", icon: <ScheduleIcon /> },
  Overdue: { tone: "error", icon: <ErrorIcon /> },
};

/** Statut avec icône et libellé ; « Clôturée » pour une échéance close, « Inconnu » si l'API n'a pas pu le calculer. */
export default function DeadlineStatusChip({ status, isClosed }: { status: DeadlineStatus | null; isClosed: boolean }) {
  if (isClosed) return <StatusChip tone="neutral" icon={<LockIcon />} label="Clôturée" />;
  if (status === null) return <StatusChip tone="neutral" icon={<HelpOutlineIcon />} label="Inconnu" />;
  return <StatusChip {...deadlineStatusChips[status]} label={deadlineStatusLabels[status]} />;
}
