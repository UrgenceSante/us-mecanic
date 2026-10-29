import { useQuery } from "@tanstack/react-query";
import type { FleetProblemFilter } from "../types/problem";
import { getProblem, listFleetProblems, listVehicleProblems } from "./problemsApi";

export const problemKeys = {
  all: ["problems"] as const,
  fleetAll: ["problems", "fleet"] as const,
  fleet: (f: FleetProblemFilter) =>
    ["problems", "fleet", { status: f.status, vehicleId: f.vehicleId, natureId: f.natureId }] as const,
  vehicle: (vehicleId: string) => ["problems", "vehicle", vehicleId] as const,
  detail: (id: string) => ["problems", "detail", id] as const,
};

export function useFleetProblems(filter: FleetProblemFilter) {
  return useQuery({
    queryKey: problemKeys.fleet(filter),
    queryFn: () => listFleetProblems(filter),
  });
}

export function useVehicleProblems(vehicleId: string) {
  return useQuery({
    queryKey: problemKeys.vehicle(vehicleId),
    queryFn: () => listVehicleProblems(vehicleId),
    enabled: vehicleId !== "",
  });
}

export function useProblem(id: string) {
  return useQuery({
    queryKey: problemKeys.detail(id),
    queryFn: () => getProblem(id),
    enabled: id !== "",
  });
}
