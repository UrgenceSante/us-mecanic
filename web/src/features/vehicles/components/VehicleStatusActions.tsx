import { Button, Stack } from "@mui/material";
import { useState } from "react";
import ConfirmDialog from "../../../shared/components/ConfirmDialog";
import type { VehicleStatus } from "../types/vehicle";

interface VehicleStatusActionsProps {
  status: VehicleStatus;
  disabled: boolean;
  onChange: (status: VehicleStatus) => void;
}

/** Transitions proposées selon le statut courant. La sortie du parc est définitive et demande confirmation. */
export default function VehicleStatusActions({ status, disabled, onChange }: VehicleStatusActionsProps) {
  const [confirming, setConfirming] = useState(false);
  if (status === "Retired") return null;

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
      <Button variant="outlined" color="error" disabled={disabled} onClick={() => setConfirming(true)}>
        Sortir du parc
      </Button>
      {confirming && (
        <ConfirmDialog
          title="Sortir le véhicule du parc"
          confirmLabel="Sortir du parc"
          submitting={disabled}
          onConfirm={() => {
            setConfirming(false);
            onChange("Retired");
          }}
          onClose={() => setConfirming(false)}
        >
          Sortir ce véhicule du parc ?
        </ConfirmDialog>
      )}
    </Stack>
  );
}
