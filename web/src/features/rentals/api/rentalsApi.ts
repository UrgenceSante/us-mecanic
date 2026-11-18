import { apiClient } from "../../../api/client";
import type { Rental, RentalInput } from "../types/rental";

const base = (vehicleId: string) => `api/vehicles/${vehicleId}/rentals`;

export async function listRentals(vehicleId: string): Promise<Rental[]> {
  const { data } = await apiClient.get<Rental[]>(base(vehicleId));
  return data;
}

export async function addRental(vehicleId: string, input: RentalInput): Promise<Rental> {
  const { data } = await apiClient.post<Rental>(base(vehicleId), input);
  return data;
}

export async function updateRental(vehicleId: string, rentalId: string, input: RentalInput): Promise<Rental> {
  const { data } = await apiClient.put<Rental>(`${base(vehicleId)}/${rentalId}`, input);
  return data;
}

export async function endRental(vehicleId: string, rentalId: string, endDate: string): Promise<Rental> {
  const { data } = await apiClient.post<Rental>(`${base(vehicleId)}/${rentalId}/end`, { endDate });
  return data;
}

export async function deleteRental(vehicleId: string, rentalId: string): Promise<void> {
  await apiClient.delete(`${base(vehicleId)}/${rentalId}`);
}
