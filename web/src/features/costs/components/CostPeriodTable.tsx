import { Box, Paper, Table, TableBody, TableCell, TableHead, TableRow, Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { CostGranularity, CostPeriod } from "../types/costSummary";
import { costFamilies, costFamilyLabels, familyTotal, formatPeriod } from "./costSummaryFormat";

interface Props {
  periods: CostPeriod[];
  granularity: CostGranularity;
}

/** Tableau par période ; sur mobile, défilement horizontal interne au tableau (pas celui de la page). */
export default function CostPeriodTable({ periods, granularity }: Props) {
  return (
    <Paper sx={{ padding: 2 }}>
      <Typography variant="h6" gutterBottom>
        Par {granularity === "month" ? "mois" : "année"}
      </Typography>
      <Box sx={{ overflowX: "auto", maxWidth: "100%" }} tabIndex={0} role="region" aria-label="Coûts par période">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Période</TableCell>
              <TableCell align="right">Total</TableCell>
              {costFamilies.map((f) => (
                <TableCell key={f} align="right">
                  {costFamilyLabels[f]}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {periods.map((p) => (
              <TableRow key={p.period}>
                <TableCell sx={{ textTransform: "capitalize" }}>{formatPeriod(p.period, granularity)}</TableCell>
                <TableCell align="right">{formatEuro(p.total)}</TableCell>
                {costFamilies.map((f) => (
                  <TableCell key={f} align="right">
                    {formatEuro(familyTotal(p.families, f))}
                  </TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Box>
    </Paper>
  );
}
