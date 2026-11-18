import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createCompany, deactivateCompany, listCompanies, renameCompany } from "./companiesApi";

export const companyKeys = {
  all: ["companies"] as const,
  list: (includeInactive: boolean) => ["companies", "list", { includeInactive }] as const,
};

export function useCompanies(includeInactive = false) {
  return useQuery({
    queryKey: companyKeys.list(includeInactive),
    queryFn: () => listCompanies(includeInactive),
  });
}

function useInvalidateCompanies() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: companyKeys.all });
}

export function useCreateCompany() {
  const onSuccess = useInvalidateCompanies();
  return useMutation({ mutationFn: (name: string) => createCompany(name), onSuccess });
}

export function useRenameCompany() {
  const onSuccess = useInvalidateCompanies();
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameCompany(id, name),
    onSuccess,
  });
}

export function useDeactivateCompany() {
  const onSuccess = useInvalidateCompanies();
  return useMutation({ mutationFn: (id: string) => deactivateCompany(id), onSuccess });
}
