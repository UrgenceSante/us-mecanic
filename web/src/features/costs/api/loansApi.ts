import { apiClient } from "../../../api/client";
import type { Loan, LoanInput } from "../types/loan";

const base = (vehicleId: string) => `api/vehicles/${vehicleId}/loans`;

export async function listLoans(vehicleId: string): Promise<Loan[]> {
  const { data } = await apiClient.get<Loan[]>(base(vehicleId));
  return data;
}

export async function addLoan(vehicleId: string, input: LoanInput): Promise<Loan> {
  const { data } = await apiClient.post<Loan>(base(vehicleId), input);
  return data;
}

export async function updateLoan(vehicleId: string, loanId: string, input: LoanInput): Promise<Loan> {
  const { data } = await apiClient.put<Loan>(`${base(vehicleId)}/${loanId}`, input);
  return data;
}

export async function deleteLoan(vehicleId: string, loanId: string): Promise<void> {
  await apiClient.delete(`${base(vehicleId)}/${loanId}`);
}
