export type ExpenseFamily = "OneOff" | "Running";
export type ExpenseSource = "Manual" | "Total" | "Ulys";

export interface Expense {
  id: string;
  vehicleId: string;
  /** Date au format AAAA-MM-JJ. */
  date: string;
  family: ExpenseFamily;
  categoryId: string | null;
  categoryName: string | null;
  amount: number;
  label: string | null;
  source: ExpenseSource;
  recordedAt: string;
  /** Date future : dépense prévue. */
  isPlanned: boolean;
}

/** La famille est fixée à « ponctuelle » côté API (valeur par défaut) : elle n'est pas envoyée. */
export interface ExpenseInput {
  date: string;
  categoryId: string;
  amount: number | null;
  label: string | null;
}

/** Seules les dépenses saisies à la main sont modifiables et supprimables. */
export const isManualExpense = (expense: Expense) => expense.source === "Manual";

/** Total en euros, arrondi au centime. */
export const expensesTotal = (expenses: Expense[]) => Math.round(expenses.reduce((sum, e) => sum + e.amount * 100, 0)) / 100;
