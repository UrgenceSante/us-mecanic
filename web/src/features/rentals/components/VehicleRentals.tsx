import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import { Alert, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material";
import { useState } from "react";
import type { Rental } from "../types/rental";
import type { Vehicle } from "../../vehicles/types/vehicle";
import { rentalErrorMessage } from "../api/rentalErrors";
import { useRentals } from "../api/rentalQueries";
import RentalDialogs, { type RentalDialog } from "./RentalDialogs";
import EndedRentals from "./EndedRentals";
import RentalList from "./RentalList";
import VehicleCurrentUser from "./VehicleCurrentUser";

interface VehicleRentalsProps {
  vehicle: Pick<Vehicle, "id" | "status" | "owner" | "currentUser">;
  /** Ouvre le formulaire du véhicule pour renseigner le propriétaire. */
  onFillOwner?: () => void;
}

/** Raison pour laquelle l'ajout est impossible (R6 : sorti du parc ; sans propriétaire), null si possible. */
function addBlockedReason(vehicle: Pick<Vehicle, "status" | "owner">): string | null {
  if (vehicle.status === "Retired") return "Véhicule sorti du parc : aucune nouvelle location.";
  if (vehicle.owner === null) return "Renseignez d'abord le propriétaire du véhicule.";
  return null;
}

const emptyText = (hasOwner: boolean) =>
  hasOwner ? "Aucune location : le véhicule est utilisé par son propriétaire." : "Aucune location.";

/** Locations en cours ou prévues d'un côté, terminées (repliées) de l'autre. */
function splitRentals(rentals: Rental[] | undefined) {
  const all = rentals ?? [];
  return { active: all.filter((r) => r.state !== "Ended"), ended: all.filter((r) => r.state === "Ended") };
}

function BlockedNotice({ reason, canFillOwner, onFillOwner }: { reason: string | null; canFillOwner: boolean; onFillOwner?: () => void }) {
  if (!reason) return null;
  return (
    <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
      <Typography id="rental-add-reason" variant="body2" color="text.secondary">
        {reason}
      </Typography>
      {canFillOwner && (
        <Button startIcon={<EditIcon />} onClick={onFillOwner}>
          Renseigner le propriétaire
        </Button>
      )}
    </Stack>
  );
}

/** Section « Propriétaire et locations » de la fiche véhicule. */
export default function VehicleRentals({ vehicle, onFillOwner }: VehicleRentalsProps) {
  const rentals = useRentals(vehicle.id);
  const [dialog, setDialog] = useState<RentalDialog | null>(null);
  const blockedReason = addBlockedReason(vehicle);
  const { active, ended } = splitRentals(rentals.data);
  const handlers = {
    onEnd: (rental: Rental) => setDialog({ kind: "end", rental }),
    onEdit: (rental: Rental) => setDialog({ kind: "edit", rental }),
    onDelete: (rental: Rental) => setDialog({ kind: "delete", rental }),
  };

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
          <Typography variant="h6" component="h2">Propriétaire et locations</Typography>
          <Stack flexGrow={1} />
          <Button
            startIcon={<AddIcon />}
            disabled={blockedReason !== null}
            aria-describedby={blockedReason ? "rental-add-reason" : undefined}
            onClick={() => setDialog({ kind: "add" })}
          >
            Ajouter une location
          </Button>
        </Stack>
        <BlockedNotice reason={blockedReason} canFillOwner={vehicle.owner === null && !!onFillOwner} onFillOwner={onFillOwner} />
        <VehicleCurrentUser currentUser={vehicle.currentUser} />
        {rentals.isLoading && <CircularProgress size={24} aria-label="Chargement des locations" />}
        {rentals.isError && <Alert severity="error">{rentalErrorMessage(rentals.error)}</Alert>}
        {rentals.data?.length === 0 && <Typography color="text.secondary">{emptyText(vehicle.owner !== null)}</Typography>}
        {active.length > 0 && <RentalList rentals={active} {...handlers} />}
        <EndedRentals rentals={ended} {...handlers} />
      </Stack>
      <RentalDialogs key={dialog?.kind ?? "none"} vehicleId={vehicle.id} ownerId={vehicle.owner?.id} dialog={dialog} onClose={() => setDialog(null)} />
    </Paper>
  );
}
