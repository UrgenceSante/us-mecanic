import { MenuItem, Stack, TextField, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { useVehicles } from "../../vehicles/api/vehicleQueries";
import type { CostSummaryParams } from "../types/costSummary";

interface Props {
  value: CostSummaryParams;
  onChange: (value: CostSummaryParams) => void;
}

export default function CostSummaryFilters({ value, onChange }: Props) {
  const vehicles = useVehicles({});
  const set = (patch: Partial<CostSummaryParams>) => onChange({ ...value, ...patch });

  return (
    <Stack direction={{ xs: "column", sm: "row" }} gap={2} alignItems={{ sm: "center" }}>
      <TextField
        label="Du"
        type="date"
        size="small"
        value={value.from}
        onChange={(e) => set({ from: e.target.value })}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <TextField
        label="Au"
        type="date"
        size="small"
        value={value.to}
        onChange={(e) => set({ to: e.target.value })}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <TextField
        select
        label="Véhicule"
        size="small"
        value={value.vehicleId}
        onChange={(e) => set({ vehicleId: e.target.value })}
        sx={{ minWidth: 200 }}
      >
        <MenuItem value="">Tout le parc</MenuItem>
        {vehicles.data?.map((v) => (
          <MenuItem key={v.id} value={v.id}>
            {v.licensePlate}
          </MenuItem>
        ))}
      </TextField>
      <ToggleButtonGroup
        exclusive
        size="small"
        value={value.granularity}
        aria-label="Granularité"
        onChange={(_, g) => g && set({ granularity: g })}
      >
        <ToggleButton value="month">Par mois</ToggleButton>
        <ToggleButton value="year">Par année</ToggleButton>
      </ToggleButtonGroup>
    </Stack>
  );
}
