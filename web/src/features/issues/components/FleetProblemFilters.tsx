import { MenuItem, Stack, TextField } from "@mui/material";
import { useProblemReferences } from "../../problem-references/api/problemReferenceQueries";
import { useVehicles } from "../../vehicles/api/vehicleQueries";
import { problemStatusLabels } from "../types/labels";
import type { FleetProblemFilter } from "../types/problem";

interface Props {
  value: FleetProblemFilter;
  onChange: (value: FleetProblemFilter) => void;
}

export default function FleetProblemFilters({ value, onChange }: Props) {
  const vehicles = useVehicles({});
  const natures = useProblemReferences("nature", true);
  const set = (patch: Partial<FleetProblemFilter>) => onChange({ ...value, ...patch });
  const vehicleOptions = vehicles.data ?? [];
  const natureOptions = natures.data ?? [];

  return (
    <Stack direction="row" gap={2} flexWrap="wrap">
      <TextField
        select
        size="small"
        label="Statut"
        sx={{ minWidth: 160 }}
        value={value.status}
        onChange={(e) => set({ status: e.target.value as FleetProblemFilter["status"] })}
      >
        <MenuItem value="">Tous (hors résolus)</MenuItem>
        {Object.entries(problemStatusLabels).map(([status, label]) => (
          <MenuItem key={status} value={status}>
            {label}
          </MenuItem>
        ))}
      </TextField>
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
      <TextField
        select
        size="small"
        label="Nature"
        sx={{ minWidth: 180 }}
        value={natureOptions.some((n) => n.id === value.natureId) ? value.natureId : ""}
        onChange={(e) => set({ natureId: e.target.value })}
      >
        <MenuItem value="">Toutes</MenuItem>
        {natureOptions.map((n) => (
          <MenuItem key={n.id} value={n.id}>
            {n.name}
          </MenuItem>
        ))}
      </TextField>
    </Stack>
  );
}
