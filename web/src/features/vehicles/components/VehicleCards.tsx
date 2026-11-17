import { Stack, Typography } from "@mui/material";
import ListCard from "../../../shared/components/ListCard";
import type { Vehicle } from "../types/vehicle";
import VehicleStatusChip from "./VehicleStatusChip";

/** Rendu mobile de la liste des véhicules : une carte cliquable par véhicule. */
export default function VehicleCards({ vehicles }: { vehicles: Vehicle[] }) {
  return (
    <Stack gap={1} aria-label="Véhicules du parc">
      {vehicles.map((v) => (
        <ListCard key={v.id} to={`/vehicles/${v.id}`} aria-label={`Véhicule ${v.licensePlate}`}>
          <Stack gap={0.5}>
            <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
              <Typography fontWeight="bold">
                {v.licensePlate}
                {v.internalNumber ? ` (${v.internalNumber})` : ""}
              </Typography>
              <VehicleStatusChip status={v.status} />
            </Stack>
            <Typography variant="body2" color="text.secondary">
              {v.typeName ?? "Type non renseigné"}
              {v.currentMileage == null ? "" : ` · ${v.currentMileage.toLocaleString("fr-FR")} km`}
            </Typography>
          </Stack>
        </ListCard>
      ))}
    </Stack>
  );
}
