import { apiClient } from "../../../api/client";
import type {
  RegisterVehicleCommand,
  Vehicle,
  VehicleDetailsInput,
  VehicleFilter,
  VehicleImportReport,
  VehicleStatus,
} from "../types/vehicle";

const BASE = "api/vehicles";

export async function listVehicles(filter: VehicleFilter): Promise<Vehicle[]> {
  const { data } = await apiClient.get<Vehicle[]>(BASE, {
    params: { search: filter.search || undefined, status: filter.status, type: filter.type },
  });
  return data;
}

export async function getVehicle(id: string): Promise<Vehicle> {
  const { data } = await apiClient.get<Vehicle>(`${BASE}/${id}`);
  return data;
}

export async function registerVehicle(command: RegisterVehicleCommand): Promise<Vehicle> {
  const { data } = await apiClient.post<Vehicle>(BASE, command);
  return data;
}

export async function updateVehicle(id: string, details: VehicleDetailsInput): Promise<Vehicle> {
  const { data } = await apiClient.put<Vehicle>(`${BASE}/${id}`, details);
  return data;
}

export async function changeVehicleStatus(id: string, status: VehicleStatus): Promise<Vehicle> {
  const { data } = await apiClient.put<Vehicle>(`${BASE}/${id}/status`, { status });
  return data;
}

export async function importVehicles(file: File, dryRun: boolean): Promise<VehicleImportReport> {
  const form = new FormData();
  form.append("file", file);
  const { data } = await apiClient.post<VehicleImportReport>(`${BASE}/import`, form, {
    params: { dryRun },
  });
  return data;
}
