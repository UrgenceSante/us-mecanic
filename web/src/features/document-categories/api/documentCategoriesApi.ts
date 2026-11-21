import { apiClient } from "../../../api/client";
import type { DocumentCategory } from "../types/documentCategory";

const BASE = "api/document-categories";

export async function listDocumentCategories(includeInactive: boolean): Promise<DocumentCategory[]> {
  const { data } = await apiClient.get<DocumentCategory[]>(BASE, { params: { includeInactive } });
  return data;
}

export async function createDocumentCategory(name: string): Promise<DocumentCategory> {
  const { data } = await apiClient.post<DocumentCategory>(BASE, { name });
  return data;
}

export async function renameDocumentCategory(id: string, name: string): Promise<DocumentCategory> {
  const { data } = await apiClient.put<DocumentCategory>(`${BASE}/${id}`, { name });
  return data;
}

export async function deactivateDocumentCategory(id: string): Promise<DocumentCategory> {
  const { data } = await apiClient.post<DocumentCategory>(`${BASE}/${id}/deactivate`);
  return data;
}
