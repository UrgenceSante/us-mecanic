import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createExpenseCategory,
  deactivateExpenseCategory,
  listExpenseCategories,
  renameExpenseCategory,
} from "./expenseCategoriesApi";

export const expenseCategoryKeys = {
  all: ["expense-categories"] as const,
  list: (includeInactive: boolean) => ["expense-categories", "list", { includeInactive }] as const,
};

export function useExpenseCategories(includeInactive = false) {
  return useQuery({
    queryKey: expenseCategoryKeys.list(includeInactive),
    queryFn: () => listExpenseCategories(includeInactive),
  });
}

function useInvalidateExpenseCategories() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: expenseCategoryKeys.all });
}

export function useCreateExpenseCategory() {
  const onSuccess = useInvalidateExpenseCategories();
  return useMutation({ mutationFn: (name: string) => createExpenseCategory(name), onSuccess });
}

export function useRenameExpenseCategory() {
  const onSuccess = useInvalidateExpenseCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameExpenseCategory(id, name),
    onSuccess,
  });
}

export function useDeactivateExpenseCategory() {
  const onSuccess = useInvalidateExpenseCategories();
  return useMutation({ mutationFn: (id: string) => deactivateExpenseCategory(id), onSuccess });
}
