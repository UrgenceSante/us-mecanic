import ActionsHeaderCell from "../../../shared/components/ActionsHeaderCell";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { IconButton, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Stack, useMediaQuery, useTheme } from "@mui/material";
import Amount from "../../../shared/components/Amount";
import ExpenseStatusChips from "./ExpenseStatusChips";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { isManualExpense, type Expense } from "../types/expense";
import ExpenseCards from "./ExpenseCards";

interface ExpenseListProps {
  expenses: Expense[];
  /** Faux : véhicule sorti du parc, aucune action. */
  editable: boolean;
  onEdit: (expense: Expense) => void;
  onDelete: (expense: Expense) => void;
}

export default function ExpenseList({ expenses, editable, onEdit, onDelete }: ExpenseListProps) {
  const compact = useMediaQuery(useTheme().breakpoints.down("sm"));
  if (compact) return <ExpenseCards expenses={expenses} editable={editable} onEdit={onEdit} onDelete={onDelete} />;
  return (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Catégorie</TableCell>
            <TableCell>Libellé</TableCell>
            <TableCell align="right">Montant</TableCell>
            {editable && <ActionsHeaderCell />}
          </TableRow>
        </TableHead>
        <TableBody>
          {expenses.map((expense) => (
            <TableRow key={expense.id} sx={expense.isPlanned ? { opacity: 0.7 } : undefined}>
              <TableCell>
                <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
                  {formatDateOnly(expense.date)}
                  <ExpenseStatusChips expense={expense} />
                </Stack>
              </TableCell>
              <TableCell>{expense.categoryName ?? "Sans catégorie"}</TableCell>
              <TableCell>{expense.label}</TableCell>
              <TableCell align="right">
                <Amount value={expense.amount} />
              </TableCell>
              {editable && (
                <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                  {isManualExpense(expense) && (
                    <>
                      <IconButton aria-label={`Modifier la dépense du ${formatDateOnly(expense.date)}`} onClick={() => onEdit(expense)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton aria-label={`Supprimer la dépense du ${formatDateOnly(expense.date)}`} onClick={() => onDelete(expense)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </>
                  )}
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
