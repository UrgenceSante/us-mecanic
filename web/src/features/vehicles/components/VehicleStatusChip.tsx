import { Chip, type ChipProps } from "@mui/material";
import { vehicleStatusLabels, type VehicleStatus } from "../types/vehicle";

const colors: Record<VehicleStatus, ChipProps["color"]> = {
  Active: "success",
  Immobilized: "warning",
  Retired: "default",
};

export default function VehicleStatusChip({ status }: { status: VehicleStatus }) {
  return <Chip size="small" color={colors[status]} label={vehicleStatusLabels[status]} />;
}
