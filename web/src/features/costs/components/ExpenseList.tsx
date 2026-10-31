import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { Chip, IconButton, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Tooltip, useMediaQuery, useTheme } from "@mui/material";
import { formatEuro } from "../../../shared/format/formatEuro";
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
            {editable && <TableCell />}
          </TableRow>
        </TableHead>
        <TableBody>
          {expenses.map((expense) => (
            <TableRow key={expense.id} sx={expense.isPlanned ? { opacity: 0.7 } : undefined}>
              <TableCell>
                {formatDateOnly(expense.date)} {expense.isPlanned && <Chip size="small" label="Prévue" color="info" sx={{ marginLeft: 1 }} />}
                {!isManualExpense(expense) && (
                  <Tooltip title="Importée automatiquement">
                    <Chip size="small" variant="outlined" label="Importée" sx={{ marginLeft: 1 }} />
                  </Tooltip>
                )}
              </TableCell>
              <TableCell>{expense.categoryName ?? "Sans catégorie"}</TableCell>
              <TableCell>{expense.label}</TableCell>
              <TableCell align="right">{formatEuro(expense.amount)}</TableCell>
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
