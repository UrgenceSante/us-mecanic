import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { ExpenseInput } from "../types/expense";
import { expenseKeys } from "./expenseKeys";
import { addExpense, deleteExpense, listExpenses, updateExpense } from "./expensesApi";

export function useExpenses(vehicleId: string) {
  return useQuery({ queryKey: expenseKeys.list(vehicleId), queryFn: () => listExpenses(vehicleId), enabled: vehicleId !== "" });
}

function useInvalidateExpenses(vehicleId: string) {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: expenseKeys.list(vehicleId) });
}

export function useAddExpense(vehicleId: string) {
  const onSuccess = useInvalidateExpenses(vehicleId);
  return useMutation({ mutationFn: (input: ExpenseInput) => addExpense(vehicleId, input), onSuccess });
}

export function useUpdateExpense(vehicleId: string) {
  const onSuccess = useInvalidateExpenses(vehicleId);
  return useMutation({
    mutationFn: ({ expenseId, input }: { expenseId: string; input: ExpenseInput }) => updateExpense(vehicleId, expenseId, input),
    onSuccess,
  });
}

export function useDeleteExpense(vehicleId: string) {
  const onSuccess = useInvalidateExpenses(vehicleId);
  return useMutation({ mutationFn: (expenseId: string) => deleteExpense(vehicleId, expenseId), onSuccess });
}
