import { Button, Stack } from "@mui/material";
import type { VehicleStatus } from "../types/vehicle";

interface VehicleStatusActionsProps {
  status: VehicleStatus;
  disabled: boolean;
  onChange: (status: VehicleStatus) => void;
}

/** Transitions proposées selon le statut courant. La sortie du parc est définitive. */
export default function VehicleStatusActions({ status, disabled, onChange }: VehicleStatusActionsProps) {
  if (status === "Retired") return null;

  const confirmRetire = () => {
    if (window.confirm("Sortir ce véhicule du parc ? Cette action est définitive.")) {
      onChange("Retired");
    }
  };

  return (
    <Stack direction="row" gap={1} flexWrap="wrap">
      {status === "Active" ? (
        <Button variant="outlined" color="warning" disabled={disabled} onClick={() => onChange("Immobilized")}>
          Immobiliser
        </Button>
      ) : (
        <Button variant="outlined" color="success" disabled={disabled} onClick={() => onChange("Active")}>
          Remettre en service
        </Button>
      )}
      <Button variant="outlined" color="inherit" disabled={disabled} onClick={confirmRetire}>
        Sortir du parc
      </Button>
    </Stack>
  );
}
