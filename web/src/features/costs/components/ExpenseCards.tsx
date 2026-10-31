import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { Chip, IconButton, Paper, Stack, Typography } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { isManualExpense, type Expense } from "../types/expense";

interface ExpenseCardsProps {
  expenses: Expense[];
  /** Faux : véhicule sorti du parc, aucune action. */
  editable: boolean;
  onEdit: (expense: Expense) => void;
  onDelete: (expense: Expense) => void;
}

const touchTarget = { width: 44, height: 44 };

/** Rendu mobile : une dépense par carte empilée. */
export default function ExpenseCards({ expenses, editable, onEdit, onDelete }: ExpenseCardsProps) {
  return (
    <Stack gap={1}>
      {expenses.map((expense) => (
        <Paper key={expense.id} variant="outlined" sx={{ padding: 1.5 }} data-testid="expense-card">
          <Stack direction="row" alignItems="flex-start" gap={1}>
            <Stack flexGrow={1} gap={0.5} minWidth={0}>
              <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
                <Typography fontWeight="bold">{formatEuro(expense.amount)}</Typography>
                {expense.isPlanned && <Chip size="small" label="Prévue" color="info" />}
              </Stack>
              <Typography variant="body2">
                {formatDateOnly(expense.date)} · {expense.categoryName ?? "Sans catégorie"}
              </Typography>
              {expense.label && (
                <Typography variant="body2" color="text.secondary">
                  {expense.label}
                </Typography>
              )}
            </Stack>
            {editable && isManualExpense(expense) && (
              <Stack direction="row" sx={{ marginTop: -0.5, marginRight: -0.5 }}>
                <IconButton sx={touchTarget} aria-label={`Modifier la dépense du ${formatDateOnly(expense.date)}`} onClick={() => onEdit(expense)}>
                  <EditIcon />
                </IconButton>
                <IconButton sx={touchTarget} aria-label={`Supprimer la dépense du ${formatDateOnly(expense.date)}`} onClick={() => onDelete(expense)}>
                  <DeleteIcon />
                </IconButton>
              </Stack>
            )}
          </Stack>
        </Paper>
      ))}
    </Stack>
  );
}
