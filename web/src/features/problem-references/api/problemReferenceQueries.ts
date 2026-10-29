import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { ProblemReferenceKind } from "../types/problemReference";
import { problemReferenceConfig } from "./referenceConfigs";
import {
  createProblemReference,
  deactivateProblemReference,
  listProblemReferences,
  renameProblemReference,
} from "./problemReferencesApi";

export const problemReferenceKeys = {
  all: (kind: ProblemReferenceKind) => ["problem-references", kind] as const,
  list: (kind: ProblemReferenceKind, includeInactive: boolean) =>
    ["problem-references", kind, "list", { includeInactive }] as const,
};

export function useProblemReferences(kind: ProblemReferenceKind, includeInactive = false) {
  const { route } = problemReferenceConfig(kind);
  return useQuery({
    queryKey: problemReferenceKeys.list(kind, includeInactive),
    queryFn: () => listProblemReferences(route, includeInactive),
  });
}

function useInvalidate(kind: ProblemReferenceKind) {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: problemReferenceKeys.all(kind) });
}

export function useCreateProblemReference(kind: ProblemReferenceKind) {
  const { route } = problemReferenceConfig(kind);
  const onSuccess = useInvalidate(kind);
  return useMutation({ mutationFn: (name: string) => createProblemReference(route, name), onSuccess });
}

export function useRenameProblemReference(kind: ProblemReferenceKind) {
  const { route } = problemReferenceConfig(kind);
  const onSuccess = useInvalidate(kind);
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameProblemReference(route, id, name),
    onSuccess,
  });
}

export function useDeactivateProblemReference(kind: ProblemReferenceKind) {
  const { route } = problemReferenceConfig(kind);
  const onSuccess = useInvalidate(kind);
  return useMutation({ mutationFn: (id: string) => deactivateProblemReference(route, id), onSuccess });
}
