import { Paper, Stack, Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { CostCategoryTotal } from "../types/costSummary";
import { costFamilies, costFamilyLabels } from "./costSummaryFormat";

export default function CostCategoryBreakdown({ categories }: { categories: CostCategoryTotal[] }) {
  return (
    <Paper sx={{ padding: 2 }}>
      <Typography variant="h6" gutterBottom>
        Par catégorie
      </Typography>
      <Stack gap={2}>
        {costFamilies.map((family) => {
          const rows = categories.filter((c) => c.family === family);
          if (rows.length === 0) return null;
          return (
            <Stack key={family} gap={0.5}>
              <Typography variant="subtitle2">{costFamilyLabels[family]}</Typography>
              {rows.map((c) => (
                <Stack key={`${family}-${c.categoryId ?? c.category}`} direction="row" justifyContent="space-between" gap={2}>
                  <Typography variant="body2" sx={{ overflowWrap: "anywhere" }}>
                    {c.category}
                  </Typography>
                  <Typography variant="body2">{formatEuro(c.total)}</Typography>
                </Stack>
              ))}
            </Stack>
          );
        })}
      </Stack>
    </Paper>
  );
}
