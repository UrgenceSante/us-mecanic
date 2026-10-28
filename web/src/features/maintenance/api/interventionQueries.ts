import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardKeys } from "../../dashboard/api/dashboardQueries";
import { mileageKeys } from "../../mileage/api/mileageQueries";
import { vehicleDeadlineKeys } from "../../vehicle-deadlines/api/vehicleDeadlineQueries";
import { vehicleKeys } from "../../vehicles/api/vehicleQueries";
import type { InterventionInput } from "../types/intervention";
import { addIntervention, listInterventions } from "./interventionsApi";

export const interventionKeys = {
  all: ["interventions"] as const,
  list: (vehicleId: string) => ["interventions", "list", vehicleId] as const,
};

export function useInterventions(vehicleId: string) {
  return useQuery({
    queryKey: interventionKeys.list(vehicleId),
    queryFn: () => listInterventions(vehicleId),
    enabled: vehicleId !== "",
  });
}

/** Une intervention réalise des échéances et peut créer un relevé : carnet, échéances, relevés, véhicules, tableau de bord. */
export function useAddIntervention(vehicleId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: InterventionInput) => addIntervention(vehicleId, input),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: interventionKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: vehicleDeadlineKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: mileageKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
        queryClient.invalidateQueries({ queryKey: dashboardKeys.all }),
      ]),
  });
}
