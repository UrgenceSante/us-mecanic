import { Paper, Stack, Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { CostPerKm, CostSummary } from "../types/costSummary";
import { costFamilies, costFamilyLabels, familyTotal } from "./costSummaryFormat";

function Card({ title, value, note }: { title: string; value: string; note?: string }) {
  return (
    <Paper sx={{ padding: 2, flex: "1 1 200px", minWidth: 0 }}>
      <Typography variant="body2" color="text.secondary">
        {title}
      </Typography>
      <Typography variant="h6">{value}</Typography>
      {note && (
        <Typography variant="caption" color="text.secondary">
          {note}
        </Typography>
      )}
    </Paper>
  );
}

function perKmCard(perKm: CostPerKm, allFleet: boolean) {
  const excluded =
    allFleet && perKm.excludedVehicles > 0
      ? `${perKm.excludedVehicles} véhicule${perKm.excludedVehicles > 1 ? "s" : ""} exclu${perKm.excludedVehicles > 1 ? "s" : ""}`
      : undefined;
  if (!perKm.computable || perKm.value === null) {
    return (
      <Card
        title="Coût au km"
        value="Non calculable"
        note={["Il faut au moins deux relevés de km et des dépenses sur la période.", excluded].filter(Boolean).join(" ")}
      />
    );
  }
  return <Card title="Coût au km" value={`${formatEuro(perKm.value)} / km`} note={excluded} />;
}

export default function CostSummaryCards({ summary }: { summary: CostSummary }) {
  return (
    <Stack direction="row" flexWrap="wrap" gap={2}>
      <Card title="Total" value={formatEuro(summary.total)} />
      {costFamilies.map((f) => (
        <Card key={f} title={costFamilyLabels[f]} value={formatEuro(familyTotal(summary.families, f))} />
      ))}
      {perKmCard(summary.costPerKm, summary.vehicleId === null)}
    </Stack>
  );
}
