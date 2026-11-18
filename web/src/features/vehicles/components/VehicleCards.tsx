import { CircularProgress, Stack, Typography } from "@mui/material";
import ListCard from "../../../shared/components/ListCard";
import UrgencyBadge from "../../../shared/components/UrgencyBadge";
import type { Vehicle } from "../types/vehicle";
import { currentUserLabel } from "./vehicleUserLabel";
import VehicleStatusChip from "./VehicleStatusChip";

interface VehicleCardsProps {
  vehicles: Vehicle[];
  loading?: boolean;
}

/** Rendu mobile de la liste des véhicules : une carte cliquable par véhicule. */
export default function VehicleCards({ vehicles, loading = false }: VehicleCardsProps) {
  if (loading) {
    return (
      <Stack alignItems="center" padding={3}>
        <CircularProgress aria-label="Chargement des véhicules" />
      </Stack>
    );
  }
  if (vehicles.length === 0) return <Typography color="text.secondary">Aucun véhicule trouvé.</Typography>;
  return (
    <Stack gap={1} role="list" aria-label="Véhicules du parc">
      {vehicles.map((v) => (
        <ListCard key={v.id} to={`/vehicles/${v.id}`}>
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
            <Typography variant="body2" color={v.owner ? "text.secondary" : "warning.main"}>
              {`Propriétaire : ${v.owner?.name ?? "à renseigner"}`}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {`Utilisateur : ${currentUserLabel(v)}`}
            </Typography>
            <UrgencyBadge alerts={v.alerts} />
          </Stack>
        </ListCard>
      ))}
    </Stack>
  );
}
