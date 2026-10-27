import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardKeys } from "../../dashboard/api/dashboardQueries";
import { mileageKeys } from "../../mileage/api/mileageQueries";
import { vehicleKeys } from "../../vehicles/api/vehicleQueries";
import type { DeadlineCompletionInput, VehicleDeadlineInput } from "../types/vehicleDeadline";
import { addVehicleDeadline, completeVehicleDeadline, listVehicleDeadlines } from "./vehicleDeadlinesApi";

export const vehicleDeadlineKeys = {
  all: ["vehicle-deadlines"] as const,
  list: (vehicleId: string) => ["vehicle-deadlines", "list", vehicleId] as const,
};

export function useVehicleDeadlines(vehicleId: string) {
  return useQuery({
    queryKey: vehicleDeadlineKeys.list(vehicleId),
    queryFn: () => listVehicleDeadlines(vehicleId),
    enabled: vehicleId !== "",
  });
}

export function useAddVehicleDeadline(vehicleId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: VehicleDeadlineInput) => addVehicleDeadline(vehicleId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: vehicleDeadlineKeys.list(vehicleId) }),
  });
}

/** Une réalisation avec km crée un relevé : on invalide tableau de bord, échéances, relevés et véhicules (km actuel). */
export function useCompleteVehicleDeadline(vehicleId: string, deadlineId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: DeadlineCompletionInput) => completeVehicleDeadline(vehicleId, deadlineId, input),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: vehicleDeadlineKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: mileageKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
        queryClient.invalidateQueries({ queryKey: dashboardKeys.all }),
      ]),
  });
}
