import { parseAmount } from "../../../shared/format/parseAmount";
import type { Expense, ExpenseInput } from "../types/expense";

export interface ExpenseFormValues {
  date: string;
  categoryId: string;
  amount: string;
  label: string;
}

export const emptyExpenseForm: ExpenseFormValues = { date: "", categoryId: "", amount: "", label: "" };

export function expenseToFormValues(expense: Expense): ExpenseFormValues {
  return {
    date: expense.date,
    categoryId: expense.categoryId ?? "",
    amount: String(expense.amount).replace(".", ","),
    label: expense.label ?? "",
  };
}

/** Date, catégorie et montant (zéro accepté) obligatoires ; libellé facultatif. */
export function isExpenseFormValid(values: ExpenseFormValues): boolean {
  return values.date !== "" && values.categoryId !== "" && parseAmount(values.amount).value !== null;
}

export function toExpenseInput(values: ExpenseFormValues): ExpenseInput {
  return {
    date: values.date,
    categoryId: values.categoryId,
    amount: parseAmount(values.amount).value,
    label: values.label.trim() === "" ? null : values.label.trim(),
  };
}
