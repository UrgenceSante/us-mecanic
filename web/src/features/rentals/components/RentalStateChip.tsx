import EventIcon from "@mui/icons-material/Event";
import KeyIcon from "@mui/icons-material/VpnKey";
import TaskAltIcon from "@mui/icons-material/TaskAlt";
import type { ReactElement } from "react";
import StatusChip, { type StatusTone } from "../../../shared/components/StatusChip";
import { rentalStateLabels, type RentalState } from "../types/rental";

const chips: Record<RentalState, { tone: StatusTone; icon: ReactElement }> = {
  Planned: { tone: "info", icon: <EventIcon /> },
  Ongoing: { tone: "success", icon: <KeyIcon /> },
  Ended: { tone: "neutral", icon: <TaskAltIcon /> },
};

export default function RentalStateChip({ state }: { state: RentalState }) {
  return <StatusChip {...chips[state]} label={rentalStateLabels[state]} category="État" />;
}
