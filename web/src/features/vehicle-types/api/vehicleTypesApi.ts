import { apiClient } from "../../../api/client";
import type { VehicleType } from "../types/vehicleType";

const BASE = "api/vehicle-types";

export async function listVehicleTypes(includeInactive: boolean): Promise<VehicleType[]> {
  const { data } = await apiClient.get<VehicleType[]>(BASE, { params: { includeInactive } });
  return data;
}

export async function createVehicleType(name: string): Promise<VehicleType> {
  const { data } = await apiClient.post<VehicleType>(BASE, { name });
  return data;
}

export async function renameVehicleType(id: string, name: string): Promise<VehicleType> {
  const { data } = await apiClient.put<VehicleType>(`${BASE}/${id}`, { name });
  return data;
}

export async function deactivateVehicleType(id: string): Promise<VehicleType> {
  const { data } = await apiClient.post<VehicleType>(`${BASE}/${id}/deactivate`);
  return data;
}
