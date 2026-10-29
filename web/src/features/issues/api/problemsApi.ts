import { apiClient } from "../../../api/client";
import type { FleetProblem, FleetProblemFilter, Problem } from "../types/problem";

export async function listFleetProblems(filter: FleetProblemFilter): Promise<FleetProblem[]> {
  const { data } = await apiClient.get<FleetProblem[]>("api/problems", {
    params: {
      status: filter.status || undefined,
      vehicleId: filter.vehicleId || undefined,
      natureId: filter.natureId || undefined,
    },
  });
  return data;
}

export async function listVehicleProblems(vehicleId: string): Promise<Problem[]> {
  const { data } = await apiClient.get<Problem[]>(`api/vehicles/${vehicleId}/problems`);
  return data;
}

export async function getProblem(id: string): Promise<Problem> {
  const { data } = await apiClient.get<Problem>(`api/problems/${id}`);
  return data;
}
