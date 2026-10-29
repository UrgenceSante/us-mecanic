import { useMutation, useQueryClient } from "@tanstack/react-query";
import { dashboardKeys } from "../../dashboard/api/dashboardQueries";
import { vehicleKeys } from "../../vehicles/api/vehicleQueries";
import type { AddProblemActionInput, AnalyzeProblemInput, UpdateProblemActionInput } from "../types/problem";
import { problemKeys } from "./problemQueries";
import { addProblemAction, analyzeProblem, closeProblemAction, updateProblemAction } from "./problemsApi";

/**
 * Après chaque écriture : détail, liste du parc, liste du véhicule et véhicules
 * (le statut immobilisé / en service peut changer) sont rafraîchis.
 */
function useInvalidateProblem(problemId: string, vehicleId: string) {
  const queryClient = useQueryClient();
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: problemKeys.detail(problemId) }),
      queryClient.invalidateQueries({ queryKey: problemKeys.fleetAll }),
      queryClient.invalidateQueries({ queryKey: problemKeys.vehicle(vehicleId) }),
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
      queryClient.invalidateQueries({ queryKey: dashboardKeys.all }),
    ]);
}

export function useAnalyzeProblem(problemId: string, vehicleId: string) {
  const onSuccess = useInvalidateProblem(problemId, vehicleId);
  return useMutation({ mutationFn: (input: AnalyzeProblemInput) => analyzeProblem(problemId, input), onSuccess });
}

export function useAddProblemAction(problemId: string, vehicleId: string) {
  const onSuccess = useInvalidateProblem(problemId, vehicleId);
  return useMutation({ mutationFn: (input: AddProblemActionInput) => addProblemAction(problemId, input), onSuccess });
}

export function useUpdateProblemAction(problemId: string, vehicleId: string) {
  const onSuccess = useInvalidateProblem(problemId, vehicleId);
  return useMutation({
    mutationFn: ({ actionId, input }: { actionId: string; input: UpdateProblemActionInput }) =>
      updateProblemAction(problemId, actionId, input),
    onSuccess,
  });
}

export function useCloseProblemAction(problemId: string, vehicleId: string) {
  const onSuccess = useInvalidateProblem(problemId, vehicleId);
  return useMutation({ mutationFn: (actionId: string) => closeProblemAction(problemId, actionId), onSuccess });
}
