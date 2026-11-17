import { apiClient } from "../../../api/client";
import type { Company } from "../types/company";

const BASE = "api/companies";

export async function listCompanies(includeInactive: boolean): Promise<Company[]> {
  const { data } = await apiClient.get<Company[]>(BASE, { params: { includeInactive } });
  return data;
}

export async function createCompany(name: string): Promise<Company> {
  const { data } = await apiClient.post<Company>(BASE, { name });
  return data;
}

export async function renameCompany(id: string, name: string): Promise<Company> {
  const { data } = await apiClient.put<Company>(`${BASE}/${id}`, { name });
  return data;
}

export async function deactivateCompany(id: string): Promise<Company> {
  const { data } = await apiClient.post<Company>(`${BASE}/${id}/deactivate`);
  return data;
}
