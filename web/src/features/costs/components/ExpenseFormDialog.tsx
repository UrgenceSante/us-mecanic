import { Alert, MenuItem, TextField } from "@mui/material";
import { useState } from "react";
import AmountField from "../../../shared/components/AmountField";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import { parseAmount } from "../../../shared/format/parseAmount";
import useFormErrors from "../../../shared/hooks/useFormErrors";
import { useExpenseCategories } from "../api/expenseCategoryQueries";
import type { ExpenseInput } from "../types/expense";
import { emptyExpenseForm, toExpenseInput, type ExpenseFormValues } from "./expenseForm";

interface ExpenseFormDialogProps {
  /** Absent pour un ajout. */
  initialValues?: ExpenseFormValues;
  submitting: boolean;
  error: string | null;
  onSubmit: (input: ExpenseInput) => void;
  onClose: () => void;
}

type Field = keyof ExpenseFormValues;

export default function ExpenseFormDialog({ initialValues, submitting, error, onSubmit, onClose }: ExpenseFormDialogProps) {
  const [values, setValues] = useState<ExpenseFormValues>(initialValues ?? emptyExpenseForm());
  const { errors, validate, clear, fieldProps } = useFormErrors<Field>();
  const set = (patch: Partial<ExpenseFormValues>) => {
    setValues((v) => ({ ...v, ...patch }));
    clear(...(Object.keys(patch) as Field[]));
  };
  const categories = useExpenseCategories(true);
  // Catégories actives, plus la catégorie déjà utilisée par la dépense même si elle est désactivée.
  const options = (categories.data ?? []).filter((c) => c.isActive || c.id === initialValues?.categoryId);

  const handleSubmit = () => {
    const amount = parseAmount(values.amount);
    const valid = validate({
      date: values.date === "" && "Indiquez la date",
      categoryId: values.categoryId === "" && "Choisissez une catégorie",
      amount: amount.error ?? (amount.value === null && "Indiquez le montant"),
    });
    if (valid) onSubmit(toExpenseInput(values));
  };

  return (
    <FormDialog
      title={initialValues ? "Modifier la dépense" : "Ajouter une dépense"}
      submitLabel={initialValues ? "Enregistrer" : "Ajouter"}
      submitting={submitting}
      error={error}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      {categories.isError && (
        <Alert severity="error" sx={{ marginBottom: 2 }}>
          Impossible de charger les catégories.
        </Alert>
      )}
      <FrenchDateField
        layout="form"
        label="Date"
        required
        value={values.date}
        onChange={(date) => set({ date })}
        {...fieldProps("date", "Une date future enregistre une dépense prévue.")}
      />
      <TextField
        select
        label="Catégorie"
        required
        fullWidth
        margin="dense"
        value={options.some((c) => c.id === values.categoryId) ? values.categoryId : ""}
        onChange={(e) => set({ categoryId: e.target.value })}
        {...fieldProps("categoryId")}
      >
        {options.map((c) => (
          <MenuItem key={c.id} value={c.id}>
            {c.isActive ? c.name : `${c.name} (désactivée)`}
          </MenuItem>
        ))}
      </TextField>
      <AmountField
        label="Montant (€)"
        required
        name="amount"
        errorText={errors.amount}
        value={values.amount}
        onChange={(amount) => set({ amount })}
      />
      <TextField
        label="Libellé"
        fullWidth
        margin="dense"
        value={values.label}
        helperText="Facultatif"
        slotProps={{ htmlInput: { maxLength: 200 } }}
        onChange={(e) => set({ label: e.target.value })}
      />
    </FormDialog>
  );
}
