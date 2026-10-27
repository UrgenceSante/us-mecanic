import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { vehicleKeys } from "../../vehicles/api/vehicleQueries";
import {
  createVehicleType,
  deactivateVehicleType,
  listVehicleTypes,
  renameVehicleType,
} from "./vehicleTypesApi";

export const vehicleTypeKeys = {
  all: ["vehicle-types"] as const,
  list: (includeInactive: boolean) => ["vehicle-types", "list", { includeInactive }] as const,
};

export function useVehicleTypes(includeInactive = false) {
  return useQuery({
    queryKey: vehicleTypeKeys.list(includeInactive),
    queryFn: () => listVehicleTypes(includeInactive),
  });
}

/** Une écriture invalide les types, et les véhicules qui affichent le nom du type. */
function useInvalidateTypes() {
  const queryClient = useQueryClient();
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: vehicleTypeKeys.all }),
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
    ]);
}

export function useCreateVehicleType() {
  const onSuccess = useInvalidateTypes();
  return useMutation({ mutationFn: createVehicleType, onSuccess });
}

export function useRenameVehicleType() {
  const onSuccess = useInvalidateTypes();
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameVehicleType(id, name),
    onSuccess,
  });
}

export function useDeactivateVehicleType() {
  const onSuccess = useInvalidateTypes();
  return useMutation({ mutationFn: deactivateVehicleType, onSuccess });
}
