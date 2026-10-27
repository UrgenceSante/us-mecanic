import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createDeadlineType,
  deactivateDeadlineType,
  getDeadlineThresholds,
  listDeadlineTypes,
  updateDeadlineThresholds,
  updateDeadlineType,
} from "./deadlineTypesApi";
import type { DeadlineTypeInput } from "../types/deadlineType";

export const deadlineTypeKeys = {
  all: ["deadline-types"] as const,
  list: (includeInactive: boolean) => ["deadline-types", "list", { includeInactive }] as const,
};

export const deadlineThresholdKeys = {
  all: ["deadline-thresholds"] as const,
};

export function useDeadlineTypes(includeInactive = false) {
  return useQuery({
    queryKey: deadlineTypeKeys.list(includeInactive),
    queryFn: () => listDeadlineTypes(includeInactive),
  });
}

export function useDeadlineThresholds() {
  return useQuery({ queryKey: deadlineThresholdKeys.all, queryFn: getDeadlineThresholds });
}

function useInvalidateTypes() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: deadlineTypeKeys.all });
}

export function useCreateDeadlineType() {
  const onSuccess = useInvalidateTypes();
  return useMutation({ mutationFn: createDeadlineType, onSuccess });
}

export function useUpdateDeadlineType() {
  const onSuccess = useInvalidateTypes();
  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: DeadlineTypeInput }) => updateDeadlineType(id, input),
    onSuccess,
  });
}

export function useDeactivateDeadlineType() {
  const onSuccess = useInvalidateTypes();
  return useMutation({ mutationFn: deactivateDeadlineType, onSuccess });
}

/** Les seuils globaux changent l'état « proche » des types sans seuil propre : on invalide aussi les types. */
export function useUpdateDeadlineThresholds() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: updateDeadlineThresholds,
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: deadlineThresholdKeys.all }),
        queryClient.invalidateQueries({ queryKey: deadlineTypeKeys.all }),
      ]),
  });
}
