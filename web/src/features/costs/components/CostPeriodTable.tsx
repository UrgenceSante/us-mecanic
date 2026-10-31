import { Box, Divider, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography, useMediaQuery } from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { CostGranularity, CostPeriod } from "../types/costSummary";
import { costFamilies, costFamilyLabels, familyTotal, formatPeriod } from "./costSummaryFormat";

interface Props {
  periods: CostPeriod[];
  granularity: CostGranularity;
}

const num = { fontVariantNumeric: "tabular-nums" } as const;

/** Mobile : une ligne empilée par période (période + total, détail par famille dessous). */
function PeriodList({ periods, granularity }: Props) {
  return (
    <Stack component="ul" sx={{ listStyle: "none", margin: 0, padding: 0 }} divider={<Divider component="li" />}>
      {periods.map((p) => (
        <Box component="li" key={p.period} sx={{ py: 1 }}>
          <Stack direction="row" justifyContent="space-between" gap={2}>
            <Typography sx={{ textTransform: "capitalize" }}>{formatPeriod(p.period, granularity)}</Typography>
            <Typography fontWeight={600} sx={num}>
              {formatEuro(p.total)}
            </Typography>
          </Stack>
          {costFamilies.map((f) => (
            <Stack key={f} direction="row" justifyContent="space-between" gap={2}>
              <Typography variant="body2" color="text.secondary">
                {costFamilyLabels[f]}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={num}>
                {formatEuro(familyTotal(p.families, f))}
              </Typography>
            </Stack>
          ))}
        </Box>
      ))}
    </Stack>
  );
}

function PeriodGrid({ periods, granularity }: Props) {
  return (
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
              <TableCell align="right" sx={num}>
                {formatEuro(p.total)}
              </TableCell>
              {costFamilies.map((f) => (
                <TableCell key={f} align="right" sx={num}>
                  {formatEuro(familyTotal(p.families, f))}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  );
}

export default function CostPeriodTable({ periods, granularity }: Props) {
  const mobile = useMediaQuery(useTheme().breakpoints.down("sm"));
  return (
    <Paper sx={{ padding: 2 }}>
      <Typography variant="h6" gutterBottom>
        Par {granularity === "month" ? "mois" : "année"}
      </Typography>
      {mobile ? <PeriodList periods={periods} granularity={granularity} /> : <PeriodGrid periods={periods} granularity={granularity} />}
    </Paper>
  );
}
