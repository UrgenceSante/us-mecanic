import { apiClient } from "../../../api/client";
import type { Intervention, InterventionInput } from "../types/intervention";

const interventionsUrl = (vehicleId: string) => `api/vehicles/${vehicleId}/interventions`;

export async function listInterventions(vehicleId: string): Promise<Intervention[]> {
  const { data } = await apiClient.get<Intervention[]>(interventionsUrl(vehicleId));
  return data;
}

export async function deleteIntervention(vehicleId: string, interventionId: string): Promise<void> {
  await apiClient.delete(`${interventionsUrl(vehicleId)}/${interventionId}`);
}

export async function addIntervention(vehicleId: string, input: InterventionInput): Promise<Intervention> {
  const { data } = await apiClient.post<Intervention>(interventionsUrl(vehicleId), input);
  return data;
}

/** Modifie le montant d'une intervention (null pour le vider). */
export async function changeInterventionAmount(vehicleId: string, interventionId: string, amount: number | null): Promise<Intervention> {
  const { data } = await apiClient.put<Intervention>(`${interventionsUrl(vehicleId)}/${interventionId}/amount`, { amount });
  return data;
}
