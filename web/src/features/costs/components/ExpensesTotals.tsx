import { Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import { expensesTotals, type Expense } from "../types/expense";

/** Totaux séparés ; « Prévues » n'apparaît que s'il y a des dépenses prévues. */
export default function ExpensesTotals({ expenses }: { expenses: Expense[] }) {
  const totals = expensesTotals(expenses);
  return (
    <Typography fontWeight="bold" textAlign="right" data-testid="expenses-total">
      Réalisées : {formatEuro(totals.done)}
      {totals.hasPlanned && ` · Prévues : ${formatEuro(totals.planned)}`}
    </Typography>
  );
}
