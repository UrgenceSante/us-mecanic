import AddIcon from "@mui/icons-material/Add";
import { Alert, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material";
import { useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { formatEuro } from "../../../shared/format/formatEuro";
import type { VehicleStatus } from "../../vehicles/types/vehicle";
import { expenseErrorMessage } from "../api/expenseErrors";
import { useAddExpense, useDeleteExpense, useExpenses, useUpdateExpense } from "../api/expenseQueries";
import { expensesTotal, type Expense, type ExpenseInput } from "../types/expense";
import DeleteExpenseDialog from "./DeleteExpenseDialog";
import ExpenseFormDialog from "./ExpenseFormDialog";
import { expenseToFormValues } from "./expenseForm";
import ExpenseList from "./ExpenseList";

interface VehicleExpensesProps {
  vehicleId: string;
  status: VehicleStatus;
}

export default function VehicleExpenses({ vehicleId, status }: VehicleExpensesProps) {
  const { notifySuccess } = useNotifSnack();
  const expenses = useExpenses(vehicleId);
  const add = useAddExpense(vehicleId);
  const update = useUpdateExpense(vehicleId);
  const remove = useDeleteExpense(vehicleId);
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Expense | null>(null);
  const [deleting, setDeleting] = useState<Expense | null>(null);
  const retired = status === "Retired";

  const closeForm = () => {
    setAdding(false);
    setEditing(null);
    add.reset();
    update.reset();
  };
  const closeDelete = () => {
    setDeleting(null);
    remove.reset();
  };
  const done = (message: string, close: () => void) => ({
    onSuccess: () => {
      notifySuccess(message);
      close();
    },
  });

  const submit = (input: ExpenseInput) =>
    editing
      ? update.mutate({ expenseId: editing.id, input }, done("Dépense modifiée", closeForm))
      : add.mutate(input, done("Dépense enregistrée", closeForm));
  const confirmDelete = () => deleting && remove.mutate(deleting.id, done("Dépense supprimée", closeDelete));
  const formMutation = editing ? update : add;

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
          <Typography variant="h6">Dépenses</Typography>
          <Stack flexGrow={1} />
          {retired ? (
            <Typography variant="body2" color="text.secondary">
              Véhicule sorti du parc : modification impossible.
            </Typography>
          ) : (
            <Button startIcon={<AddIcon />} onClick={() => setAdding(true)}>
              Ajouter une dépense
            </Button>
          )}
        </Stack>
        {expenses.isLoading && <CircularProgress size={24} aria-label="Chargement des dépenses" />}
        {expenses.isError && <Alert severity="error">{expenseErrorMessage(expenses.error)}</Alert>}
        {expenses.data?.length === 0 && <Typography color="text.secondary">Aucune dépense enregistrée pour ce véhicule.</Typography>}
        {expenses.data && expenses.data.length > 0 && (
          <>
            <ExpenseList expenses={expenses.data} editable={!retired} onEdit={setEditing} onDelete={setDeleting} />
            <Typography fontWeight="bold" textAlign="right" data-testid="expenses-total">
              Total : {formatEuro(expensesTotal(expenses.data))}
            </Typography>
          </>
        )}
      </Stack>
      {(adding || editing) && (
        <ExpenseFormDialog
          initialValues={editing ? expenseToFormValues(editing) : undefined}
          submitting={formMutation.isPending}
          error={formMutation.isError ? expenseErrorMessage(formMutation.error) : null}
          onSubmit={submit}
          onClose={closeForm}
        />
      )}
      {deleting && (
        <DeleteExpenseDialog
          expense={deleting}
          submitting={remove.isPending}
          error={remove.isError ? expenseErrorMessage(remove.error) : null}
          onConfirm={confirmDelete}
          onClose={closeDelete}
        />
      )}
    </Paper>
  );
}
