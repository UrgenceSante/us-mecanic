import { Box, Paper, Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { CostPerKm, CostSummary } from "../types/costSummary";
import { costFamilies, costFamilyLabels, excludedLabel, familyTotal } from "./costSummaryFormat";

interface CardProps {
  title: string;
  value: string;
  notes?: string[];
  highlight?: boolean;
  fullWidth?: boolean;
  /** Pleine largeur sur mobile seulement (évite une case orpheline dans la grille 2 colonnes). */
  fullOnMobile?: boolean;
}

function Card({ title, value, notes = [], highlight, fullWidth, fullOnMobile }: CardProps) {
  return (
    <Paper
      sx={{
        padding: 2,
        minWidth: 0,
        gridColumn: fullWidth || fullOnMobile ? { xs: "1 / -1", md: "auto" } : undefined,
        ...(highlight && { bgcolor: "primary.main", color: "primary.contrastText" }),
      }}
    >
      <Typography variant="body2" color={highlight ? "inherit" : "text.secondary"}>
        {title}
      </Typography>
      <Typography variant={highlight ? "h4" : "h6"} component="p" sx={{ fontVariantNumeric: "tabular-nums" }}>
        {value}
      </Typography>
      {notes.map((n) => (
        <Typography key={n} variant="caption" display="block" sx={{ textWrap: "balance" }} color={highlight ? "inherit" : "text.secondary"}>
          {n}
        </Typography>
      ))}
    </Paper>
  );
}

function perKmCard(perKm: CostPerKm, allFleet: boolean) {
  const notes: string[] = [];
  if (!perKm.computable || perKm.value === null) {
    notes.push("Il faut au moins deux relevés kilométriques sur la période.");
  } else {
    notes.push(`${new Intl.NumberFormat("fr-FR").format(perKm.kilometers)} km parcourus`);
  }
  if (allFleet && perKm.excludedVehicles > 0) notes.push(excludedLabel(perKm.excludedVehicles));
  const value = perKm.computable && perKm.value !== null ? `${formatEuro(perKm.value)} / km` : "Non calculable";
  return <Card title="Coût au km" value={value} notes={notes} fullWidth />;
}

/** Total et coût au km en pleine largeur sur mobile, familles en grille 2 colonnes. */
export default function CostSummaryCards({ summary }: { summary: CostSummary }) {
  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: { xs: "repeat(2, minmax(0, 1fr))", md: "repeat(5, minmax(0, 1fr))" },
      }}
    >
      <Card title="Total" value={formatEuro(summary.total)} highlight fullWidth />
      {costFamilies.map((f) => (
        <Card key={f} title={costFamilyLabels[f]} value={formatEuro(familyTotal(summary.families, f))} fullOnMobile={f === "Loan"} />
      ))}
      {perKmCard(summary.costPerKm, summary.vehicleId === null)}
    </Box>
  );
}
