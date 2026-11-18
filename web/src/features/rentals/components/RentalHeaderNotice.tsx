import WarningIcon from "@mui/icons-material/Warning";
import { Stack, Typography } from "@mui/material";
import StatusChip from "../../../shared/components/StatusChip";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { VehicleStatus } from "../../vehicles/types/vehicle";
import { useRentals } from "../api/rentalQueries";
import { ongoingRental } from "../types/rental";

/** En-tête de la fiche : « Loué à B depuis le JJ/MM/AAAA », et le signal à traiter si le véhicule est sorti du parc. */
export default function RentalHeaderNotice({ vehicleId, status }: { vehicleId: string; status: VehicleStatus }) {
  const rental = ongoingRental(useRentals(vehicleId).data);
  if (!rental) return null;
  return (
    <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
      <Typography>{`Loué à ${rental.tenant.name} depuis le ${formatDateOnly(rental.startDate)}`}</Typography>
      {status === "Retired" && (
        <StatusChip tone="warning" icon={<WarningIcon />} label="Location en cours à terminer" category="Alerte" />
      )}
    </Stack>
  );
}
