import { Paper, Stack, Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { CostCategoryTotal } from "../types/costSummary";
import { costFamilies, costFamilyLabels } from "./costSummaryFormat";

const num = { fontVariantNumeric: "tabular-nums" } as const;

export default function CostCategoryBreakdown({ categories }: { categories: CostCategoryTotal[] }) {
  return (
    <Paper sx={{ padding: 2, maxWidth: 600, width: "100%" }}>
      <Typography variant="h6" gutterBottom>
        Par catégorie
      </Typography>
      <Stack gap={2}>
        {costFamilies.map((family) => {
          const rows = categories.filter((c) => c.family === family);
          if (rows.length === 0) return null;
          const subtotal = rows.reduce((sum, c) => sum + c.total, 0);
          return (
            <Stack key={family} gap={0.5}>
              <Stack direction="row" justifyContent="space-between" gap={2}>
                <Typography variant="subtitle2">{costFamilyLabels[family]}</Typography>
                <Typography variant="subtitle2" sx={num} aria-label={`Sous-total ${costFamilyLabels[family]}`}>
                  {formatEuro(subtotal)}
                </Typography>
              </Stack>
              {rows.map((c) => (
                <Stack key={`${family}-${c.categoryId ?? c.category}`} direction="row" justifyContent="space-between" gap={2} pl={1.5}>
                  <Typography variant="body2" sx={{ overflowWrap: "anywhere" }}>
                    {c.category}
                  </Typography>
                  <Typography variant="body2" sx={num}>
                    {formatEuro(c.total)}
                  </Typography>
                </Stack>
              ))}
            </Stack>
          );
        })}
      </Stack>
    </Paper>
  );
}
