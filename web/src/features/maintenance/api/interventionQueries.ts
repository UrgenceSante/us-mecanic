import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { dashboardKeys } from "../../dashboard/api/dashboardQueries";
import { mileageErrorCode } from "../../mileage/api/mileageErrors";
import { mileageKeys } from "../../mileage/api/mileageQueries";
import { vehicleDeadlineKeys } from "../../vehicle-deadlines/api/vehicleDeadlineKeys";
import { vehicleKeys } from "../../vehicles/api/vehicleQueries";
import type { InterventionInput } from "../types/intervention";
import { interventionKeys } from "./interventionKeys";
import { addIntervention, deleteIntervention, listInterventions } from "./interventionsApi";

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

/** Suppression : échéances non recalculées et relevé conservé, mais carnet, échéances et tableau de bord se rafraîchissent. */
export function useDeleteIntervention(vehicleId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (interventionId: string) => deleteIntervention(vehicleId, interventionId),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: interventionKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: vehicleDeadlineKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
        queryClient.invalidateQueries({ queryKey: dashboardKeys.all }),
      ]),
    // Déjà supprimée ailleurs : le carnet affiché est périmé, on le rafraîchit sans changer l'erreur exposée.
    onError: (error) =>
      mileageErrorCode(error) === "intervention.not_found"
        ? queryClient.invalidateQueries({ queryKey: interventionKeys.list(vehicleId) })
        : undefined,
  });
}
