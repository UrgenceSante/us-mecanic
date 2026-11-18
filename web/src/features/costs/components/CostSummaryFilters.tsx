import { Box, Button, MenuItem, Stack, TextField, ToggleButton, ToggleButtonGroup } from "@mui/material";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import CompanySelect from "../../companies/components/CompanySelect";
import { useVehicles } from "../../vehicles/api/vehicleQueries";
import type { CostSummaryParams } from "../types/costSummary";
import { type PeriodShortcut, periodShortcutLabels, shortcutRange } from "./costSummaryFormat";

interface Props {
  value: CostSummaryParams;
  onChange: (value: CostSummaryParams) => void;
}

const shortcuts = Object.keys(periodShortcutLabels) as PeriodShortcut[];
// Cible tactile d'au moins 44 px sur téléphone.
const touchOnMobile = { minHeight: { xs: 44, sm: "auto" } };

export default function CostSummaryFilters({ value, onChange }: Props) {
  const vehicles = useVehicles({});
  const set = (patch: Partial<CostSummaryParams>) => onChange({ ...value, ...patch });

  return (
    <Stack gap={2}>
      <Stack direction={{ xs: "column", sm: "row" }} gap={2} alignItems={{ sm: "center" }}>
        <Stack direction="row" gap={2}>
          <FrenchDateField label="Du" value={value.from} onChange={(from) => set({ from })} />
          <FrenchDateField label="Au" value={value.to} onChange={(to) => set({ to })} />
        </Stack>
        <TextField
          select
          label="Véhicule"
          size="small"
          value={value.vehicleId}
          onChange={(e) => set({ vehicleId: e.target.value })}
          slotProps={{ inputLabel: { shrink: true }, select: { displayEmpty: true } }}
          sx={{ minWidth: { sm: 240 } }}
        >
          <MenuItem value="">Tout le parc</MenuItem>
          {vehicles.data?.map((v) => (
            <MenuItem key={v.id} value={v.id}>
              {[v.licensePlate, [v.brand, v.model].filter(Boolean).join(" ")].filter(Boolean).join(" · ")}
            </MenuItem>
          ))}
        </TextField>
      </Stack>
      <Stack direction={{ xs: "column", sm: "row" }} gap={2} alignItems={{ sm: "flex-start" }}>
        <Box sx={{ width: { sm: 240 } }}>
          <CompanySelect
            label="Société propriétaire"
            emptyLabel="Toutes"
            size="small"
            includeInactive
            value={value.ownerCompanyId}
            onChange={(ownerCompanyId) => set({ ownerCompanyId })}
          />
        </Box>
        <Box sx={{ width: { sm: 240 } }}>
          <CompanySelect
            label="Société locataire"
            emptyLabel="Toutes"
            size="small"
            includeInactive
            value={value.lesseeCompanyId}
            onChange={(lesseeCompanyId) => set({ lesseeCompanyId })}
            helperText="Ne compte que les dépenses faites pendant une location à cette société. Les emprunts restent attribués au propriétaire, et le coût au km n'est pas calculé."
          />
        </Box>
      </Stack>
      <Stack direction="row" gap={1} flexWrap="wrap" alignItems="center">
        {shortcuts.map((s) => (
          <Button key={s} size="small" variant="outlined" sx={touchOnMobile} onClick={() => set(shortcutRange(s))}>
            {periodShortcutLabels[s]}
          </Button>
        ))}
        <ToggleButtonGroup
          exclusive
          size="small"
          value={value.granularity}
          aria-label="Granularité"
          onChange={(_, g) => g && set({ granularity: g })}
          sx={{ ml: { sm: "auto" } }}
        >
          <ToggleButton value="month" sx={touchOnMobile}>Par mois</ToggleButton>
          <ToggleButton value="year" sx={touchOnMobile}>Par année</ToggleButton>
        </ToggleButtonGroup>
      </Stack>
    </Stack>
  );
}
