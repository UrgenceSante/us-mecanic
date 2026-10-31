import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";
import AmountField from "../../../shared/components/AmountField";
import { useExpenseCategories } from "../api/expenseCategoryQueries";
import type { ExpenseInput } from "../types/expense";
import { emptyExpenseForm, isExpenseFormValid, toExpenseInput, type ExpenseFormValues } from "./expenseForm";

interface ExpenseFormDialogProps {
  /** Absent pour un ajout. */
  initialValues?: ExpenseFormValues;
  submitting: boolean;
  error: string | null;
  onSubmit: (input: ExpenseInput) => void;
  onClose: () => void;
}

export default function ExpenseFormDialog({ initialValues, submitting, error, onSubmit, onClose }: ExpenseFormDialogProps) {
  const [values, setValues] = useState<ExpenseFormValues>(initialValues ?? emptyExpenseForm);
  const set = (patch: Partial<ExpenseFormValues>) => setValues((v) => ({ ...v, ...patch }));
  const categories = useExpenseCategories(true);
  // Catégories actives, plus la catégorie déjà utilisée par la dépense même si elle est désactivée.
  const options = (categories.data ?? []).filter((c) => c.isActive || c.id === initialValues?.categoryId);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (isExpenseFormValid(values)) onSubmit(toExpenseInput(values));
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{initialValues ? "Modifier la dépense" : "Ajouter une dépense"}</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          {categories.isError && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              Impossible de charger les catégories.
            </Alert>
          )}
          <TextField
            label="Date"
            type="date"
            required
            fullWidth
            margin="dense"
            value={values.date}
            helperText="Une date future enregistre une dépense prévue."
            slotProps={{ inputLabel: { shrink: true } }}
            onChange={(e) => set({ date: e.target.value })}
          />
          <TextField
            select
            label="Catégorie"
            required
            fullWidth
            margin="dense"
            value={options.some((c) => c.id === values.categoryId) ? values.categoryId : ""}
            onChange={(e) => set({ categoryId: e.target.value })}
          >
            {options.map((c) => (
              <MenuItem key={c.id} value={c.id}>
                {c.isActive ? c.name : `${c.name} (désactivée)`}
              </MenuItem>
            ))}
          </TextField>
          <AmountField label="Montant (€)" required value={values.amount} onChange={(v) => set({ amount: v })} />
          <TextField
            label="Libellé"
            fullWidth
            margin="dense"
            value={values.label}
            helperText="Facultatif"
            slotProps={{ htmlInput: { maxLength: 200 } }}
            onChange={(e) => set({ label: e.target.value })}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={submitting || !isExpenseFormValid(values)}>
            {initialValues ? "Enregistrer" : "Ajouter"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
