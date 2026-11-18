import AddIcon from "@mui/icons-material/Add";
import { Alert, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material";
import { useState } from "react";
import type { Vehicle } from "../../vehicles/types/vehicle";
import { rentalErrorMessage } from "../api/rentalErrors";
import { useRentals } from "../api/rentalQueries";
import RentalDialogs, { type RentalDialog } from "./RentalDialogs";
import RentalList from "./RentalList";
import VehicleCurrentUser from "./VehicleCurrentUser";

interface VehicleRentalsProps {
  vehicle: Pick<Vehicle, "id" | "status" | "owner" | "currentUser">;
}

/** Raison pour laquelle l'ajout est impossible (R6 : sorti du parc ; sans propriétaire), null si possible. */
function addBlockedReason(vehicle: Pick<Vehicle, "status" | "owner">): string | null {
  if (vehicle.status === "Retired") return "Véhicule sorti du parc : aucune nouvelle location.";
  if (vehicle.owner === null) return "Renseignez d'abord le propriétaire du véhicule.";
  return null;
}

/** Section « Propriétaire et locations » de la fiche véhicule. */
export default function VehicleRentals({ vehicle }: VehicleRentalsProps) {
  const rentals = useRentals(vehicle.id);
  const [dialog, setDialog] = useState<RentalDialog | null>(null);
  const blockedReason = addBlockedReason(vehicle);
  const emptyText = vehicle.owner
    ? "Aucune location : le véhicule est utilisé par son propriétaire."
    : "Aucune location. Propriétaire à renseigner.";

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
        {blockedReason && (
          <Typography id="rental-add-reason" variant="body2" color="text.secondary">
            {blockedReason}
          </Typography>
        )}
        <VehicleCurrentUser owner={vehicle.owner} currentUser={vehicle.currentUser} />
        {rentals.isLoading && <CircularProgress size={24} aria-label="Chargement des locations" />}
        {rentals.isError && <Alert severity="error">{rentalErrorMessage(rentals.error)}</Alert>}
        {rentals.data?.length === 0 && <Typography color="text.secondary">{emptyText}</Typography>}
        {rentals.data && rentals.data.length > 0 && (
          <RentalList
            rentals={rentals.data}
            onEnd={(rental) => setDialog({ kind: "end", rental })}
            onEdit={(rental) => setDialog({ kind: "edit", rental })}
            onDelete={(rental) => setDialog({ kind: "delete", rental })}
          />
        )}
      </Stack>
      <RentalDialogs key={dialog?.kind ?? "none"} vehicleId={vehicle.id} dialog={dialog} onClose={() => setDialog(null)} />
    </Paper>
  );
}
