import { apiClient } from "../../../api/client";
import type { Garage } from "../types/garage";

const BASE = "api/garages";

export async function listGarages(includeInactive: boolean): Promise<Garage[]> {
  const { data } = await apiClient.get<Garage[]>(BASE, { params: { includeInactive } });
  return data;
}

export async function createGarage(name: string): Promise<Garage> {
  const { data } = await apiClient.post<Garage>(BASE, { name });
  return data;
}

export async function renameGarage(id: string, name: string): Promise<Garage> {
  const { data } = await apiClient.put<Garage>(`${BASE}/${id}`, { name });
  return data;
}

export async function deactivateGarage(id: string): Promise<Garage> {
  const { data } = await apiClient.post<Garage>(`${BASE}/${id}/deactivate`);
  return data;
}
