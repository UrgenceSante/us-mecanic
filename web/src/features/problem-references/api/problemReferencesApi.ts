import { apiClient } from "../../../api/client";
import type { ProblemReference } from "../types/problemReference";

export async function listProblemReferences(route: string, includeInactive: boolean): Promise<ProblemReference[]> {
  const { data } = await apiClient.get<ProblemReference[]>(route, { params: { includeInactive } });
  return data;
}

export async function createProblemReference(route: string, name: string): Promise<ProblemReference> {
  const { data } = await apiClient.post<ProblemReference>(route, { name });
  return data;
}

export async function renameProblemReference(route: string, id: string, name: string): Promise<ProblemReference> {
  const { data } = await apiClient.put<ProblemReference>(`${route}/${id}`, { name });
  return data;
}

export async function deactivateProblemReference(route: string, id: string): Promise<ProblemReference> {
  const { data } = await apiClient.post<ProblemReference>(`${route}/${id}/deactivate`);
  return data;
}
