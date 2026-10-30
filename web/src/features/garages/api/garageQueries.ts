import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createGarage, deactivateGarage, listGarages, renameGarage } from "./garagesApi";

export const garageKeys = {
  all: ["garages"] as const,
  list: (includeInactive: boolean) => ["garages", "list", { includeInactive }] as const,
};

export function useGarages(includeInactive = false) {
  return useQuery({
    queryKey: garageKeys.list(includeInactive),
    queryFn: () => listGarages(includeInactive),
  });
}

function useInvalidateGarages() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: garageKeys.all });
}

export function useCreateGarage() {
  const onSuccess = useInvalidateGarages();
  return useMutation({ mutationFn: (name: string) => createGarage(name), onSuccess });
}

export function useRenameGarage() {
  const onSuccess = useInvalidateGarages();
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameGarage(id, name),
    onSuccess,
  });
}

export function useDeactivateGarage() {
  const onSuccess = useInvalidateGarages();
  return useMutation({ mutationFn: (id: string) => deactivateGarage(id), onSuccess });
}
