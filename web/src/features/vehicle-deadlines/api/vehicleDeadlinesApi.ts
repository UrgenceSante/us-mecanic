import { apiClient } from "../../../api/client";
import type { DeadlineCompletionInput, VehicleDeadline, VehicleDeadlineInput } from "../types/vehicleDeadline";

const deadlinesUrl = (vehicleId: string) => `api/vehicles/${vehicleId}/deadlines`;

export async function listVehicleDeadlines(vehicleId: string): Promise<VehicleDeadline[]> {
  const { data } = await apiClient.get<VehicleDeadline[]>(deadlinesUrl(vehicleId));
  return data;
}

export async function addVehicleDeadline(vehicleId: string, input: VehicleDeadlineInput): Promise<VehicleDeadline> {
  const { data } = await apiClient.post<VehicleDeadline>(deadlinesUrl(vehicleId), input);
  return data;
}

export async function completeVehicleDeadline(
  vehicleId: string,
  deadlineId: string,
  input: DeadlineCompletionInput,
): Promise<VehicleDeadline> {
  const { data } = await apiClient.post<VehicleDeadline>(`${deadlinesUrl(vehicleId)}/${deadlineId}/completions`, input);
  return data;
}
