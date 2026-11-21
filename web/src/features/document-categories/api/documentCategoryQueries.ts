import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createDocumentCategory,
  deactivateDocumentCategory,
  listDocumentCategories,
  renameDocumentCategory,
} from "./documentCategoriesApi";

export const documentCategoryKeys = {
  all: ["document-categories"] as const,
  list: (includeInactive: boolean) => ["document-categories", "list", { includeInactive }] as const,
};

export function useDocumentCategories(includeInactive = false) {
  return useQuery({
    queryKey: documentCategoryKeys.list(includeInactive),
    queryFn: () => listDocumentCategories(includeInactive),
  });
}

function useInvalidateDocumentCategories() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: documentCategoryKeys.all });
}

export function useCreateDocumentCategory() {
  const onSuccess = useInvalidateDocumentCategories();
  return useMutation({ mutationFn: (name: string) => createDocumentCategory(name), onSuccess });
}

export function useRenameDocumentCategory() {
  const onSuccess = useInvalidateDocumentCategories();
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameDocumentCategory(id, name),
    onSuccess,
  });
}

export function useDeactivateDocumentCategory() {
  const onSuccess = useInvalidateDocumentCategories();
  return useMutation({ mutationFn: (id: string) => deactivateDocumentCategory(id), onSuccess });
}
