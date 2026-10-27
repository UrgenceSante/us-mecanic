import { MenuItem, Stack, TextField } from "@mui/material";
import { useDeadlineTypes } from "../../deadline-types/api/deadlineTypeQueries";
import { useVehicles } from "../../vehicles/api/vehicleQueries";
import VehicleTypeSelect from "../../vehicle-types/components/VehicleTypeSelect";
import type { DashboardFilter } from "../types/dashboard";

interface DashboardFiltersProps {
  value: DashboardFilter;
  onChange: (value: DashboardFilter) => void;
}

export default function DashboardFilters({ value, onChange }: DashboardFiltersProps) {
  const vehicles = useVehicles({});
  const deadlineTypes = useDeadlineTypes(true);
  const set = (patch: Partial<DashboardFilter>) => onChange({ ...value, ...patch });
  const vehicleOptions = vehicles.data ?? [];
  const typeOptions = deadlineTypes.data ?? [];

  return (
    <Stack direction="row" gap={2} flexWrap="wrap">
      <TextField
        select
        size="small"
        label="Véhicule"
        sx={{ minWidth: 180 }}
        value={vehicleOptions.some((v) => v.id === value.vehicleId) ? value.vehicleId : ""}
        onChange={(e) => set({ vehicleId: e.target.value })}
      >
        <MenuItem value="">Tous</MenuItem>
        {vehicleOptions.map((v) => (
          <MenuItem key={v.id} value={v.id}>
            {v.licensePlate}
          </MenuItem>
        ))}
      </TextField>
      <VehicleTypeSelect
        label="Type de véhicule"
        size="small"
        minWidth={180}
        emptyLabel="Tous"
        includeInactive
        value={value.vehicleTypeId}
        onChange={(vehicleTypeId) => set({ vehicleTypeId })}
      />
      <TextField
        select
        size="small"
        label="Type d'échéance"
        sx={{ minWidth: 180 }}
        value={typeOptions.some((t) => t.id === value.deadlineTypeId) ? value.deadlineTypeId : ""}
        onChange={(e) => set({ deadlineTypeId: e.target.value })}
      >
        <MenuItem value="">Tous</MenuItem>
        {typeOptions.map((t) => (
          <MenuItem key={t.id} value={t.id}>
            {t.name}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        select
        size="small"
        label="Statut"
        sx={{ minWidth: 140 }}
        value={value.status}
        onChange={(e) => set({ status: e.target.value as DashboardFilter["status"] })}
      >
        <MenuItem value="">Tous</MenuItem>
        <MenuItem value="Overdue">Dépassée</MenuItem>
        <MenuItem value="DueSoon">Proche</MenuItem>
      </TextField>
    </Stack>
  );
}
