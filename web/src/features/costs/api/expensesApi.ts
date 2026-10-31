import { apiClient } from "../../../api/client";
import type { Expense, ExpenseInput } from "../types/expense";

const base = (vehicleId: string) => `api/vehicles/${vehicleId}/expenses`;

export async function listExpenses(vehicleId: string): Promise<Expense[]> {
  const { data } = await apiClient.get<Expense[]>(base(vehicleId));
  return data;
}

export async function addExpense(vehicleId: string, input: ExpenseInput): Promise<Expense> {
  const { data } = await apiClient.post<Expense>(base(vehicleId), input);
  return data;
}

export async function updateExpense(vehicleId: string, expenseId: string, input: ExpenseInput): Promise<Expense> {
  const { data } = await apiClient.put<Expense>(`${base(vehicleId)}/${expenseId}`, input);
  return data;
}

export async function deleteExpense(vehicleId: string, expenseId: string): Promise<void> {
  await apiClient.delete(`${base(vehicleId)}/${expenseId}`);
}
