import { apiClient } from "../../../api/client";
import type { MileageReading, MileageReadingInput } from "../types/mileageReading";

const readingsUrl = (vehicleId: string) => `api/vehicles/${vehicleId}/mileage-readings`;

export async function listMileageReadings(vehicleId: string): Promise<MileageReading[]> {
  const { data } = await apiClient.get<MileageReading[]>(readingsUrl(vehicleId));
  return data;
}

export async function addMileageReading(vehicleId: string, input: MileageReadingInput): Promise<MileageReading> {
  const { data } = await apiClient.post<MileageReading>(readingsUrl(vehicleId), input);
  return data;
}
