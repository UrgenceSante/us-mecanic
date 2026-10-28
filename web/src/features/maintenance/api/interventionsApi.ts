import { apiClient } from "../../../api/client";
import type { Intervention, InterventionInput } from "../types/intervention";

const interventionsUrl = (vehicleId: string) => `api/vehicles/${vehicleId}/interventions`;

export async function listInterventions(vehicleId: string): Promise<Intervention[]> {
  const { data } = await apiClient.get<Intervention[]>(interventionsUrl(vehicleId));
  return data;
}

export async function addIntervention(vehicleId: string, input: InterventionInput): Promise<Intervention> {
  const { data } = await apiClient.post<Intervention>(interventionsUrl(vehicleId), input);
  return data;
}
