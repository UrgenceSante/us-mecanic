import { apiClient } from "../../../api/client";
import type { ExpenseCategory } from "../types/expenseCategory";

const BASE = "api/expense-categories";

export async function listExpenseCategories(includeInactive: boolean): Promise<ExpenseCategory[]> {
  const { data } = await apiClient.get<ExpenseCategory[]>(BASE, { params: { includeInactive } });
  return data;
}

export async function createExpenseCategory(name: string): Promise<ExpenseCategory> {
  const { data } = await apiClient.post<ExpenseCategory>(BASE, { name });
  return data;
}

export async function renameExpenseCategory(id: string, name: string): Promise<ExpenseCategory> {
  const { data } = await apiClient.put<ExpenseCategory>(`${BASE}/${id}`, { name });
  return data;
}

export async function deactivateExpenseCategory(id: string): Promise<ExpenseCategory> {
  const { data } = await apiClient.post<ExpenseCategory>(`${BASE}/${id}/deactivate`);
  return data;
}
