import ArchiveIcon from "@mui/icons-material/Archive";
import BlockIcon from "@mui/icons-material/Block";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import type { ReactElement } from "react";
import StatusChip, { type StatusTone } from "../../../shared/components/StatusChip";
import { vehicleStatusLabels, type VehicleStatus } from "../types/vehicle";

const vehicleStatusChips: Record<VehicleStatus, { tone: StatusTone; icon: ReactElement }> = {
  Active: { tone: "success", icon: <CheckCircleIcon /> },
  Immobilized: { tone: "error", icon: <BlockIcon /> },
  Retired: { tone: "neutral", icon: <ArchiveIcon /> },
};

export default function VehicleStatusChip({ status }: { status: VehicleStatus }) {
  return <StatusChip {...vehicleStatusChips[status]} label={vehicleStatusLabels[status]} />;
}
