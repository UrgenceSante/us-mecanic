import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { RegisterVehicleCommand, VehicleDetailsInput, VehicleFilter, VehicleStatus } from "../types/vehicle";
import {
  changeVehicleStatus,
  getVehicle,
  importVehicles,
  listVehicles,
  registerVehicle,
  updateVehicle,
} from "./vehiclesApi";

export const vehicleKeys = {
  all: ["vehicles"] as const,
  list: (filter: VehicleFilter) => ["vehicles", "list", filter.search ?? "", filter.status ?? "", filter.type ?? ""] as const,
  detail: (id: string) => ["vehicles", "detail", id] as const,
};

export function useVehicles(filter: VehicleFilter) {
  return useQuery({ queryKey: vehicleKeys.list(filter), queryFn: () => listVehicles(filter) });
}

export function useVehicle(id: string) {
  return useQuery({ queryKey: vehicleKeys.detail(id), queryFn: () => getVehicle(id), enabled: id !== "" });
}

/** Toute écriture invalide l'ensemble des requêtes véhicules (listes et fiches). */
function useInvalidateVehicles() {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
}

export function useRegisterVehicle() {
  const onSuccess = useInvalidateVehicles();
  return useMutation({ mutationFn: (command: RegisterVehicleCommand) => registerVehicle(command), onSuccess });
}

export function useUpdateVehicle(id: string) {
  const onSuccess = useInvalidateVehicles();
  return useMutation({
    mutationFn: (details: VehicleDetailsInput) => updateVehicle(id, details),
    onSuccess,
  });
}

export function useChangeVehicleStatus(id: string) {
  const onSuccess = useInvalidateVehicles();
  return useMutation({
    mutationFn: (status: VehicleStatus) => changeVehicleStatus(id, status),
    onSuccess,
  });
}

export function useImportVehicles() {
  const invalidate = useInvalidateVehicles();
  return useMutation({
    mutationFn: ({ file, dryRun }: { file: File; dryRun: boolean }) => importVehicles(file, dryRun),
    onSuccess: (report) => (report.dryRun ? undefined : invalidate()),
  });
}
