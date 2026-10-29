import { apiClient } from "../../../api/client";
import type {
  AddProblemActionInput,
  AnalyzeProblemInput,
  FleetProblem,
  FleetProblemFilter,
  Problem,
  ProblemAction,
  UpdateProblemActionInput,
} from "../types/problem";

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

export async function analyzeProblem(id: string, input: AnalyzeProblemInput): Promise<Problem> {
  const { data } = await apiClient.put<Problem>(`api/problems/${id}/analysis`, input);
  return data;
}

export async function addProblemAction(id: string, input: AddProblemActionInput): Promise<ProblemAction> {
  const { data } = await apiClient.post<ProblemAction>(`api/problems/${id}/actions`, input);
  return data;
}

export async function updateProblemAction(
  id: string,
  actionId: string,
  input: UpdateProblemActionInput,
): Promise<ProblemAction> {
  const { data } = await apiClient.put<ProblemAction>(`api/problems/${id}/actions/${actionId}`, input);
  return data;
}

export async function closeProblemAction(id: string, actionId: string): Promise<ProblemAction> {
  const { data } = await apiClient.post<ProblemAction>(`api/problems/${id}/actions/${actionId}/close`);
  return data;
}
