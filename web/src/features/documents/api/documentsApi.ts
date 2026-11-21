import { apiClient } from "../../../api/client";
import type { DocumentUpload, VehicleDocument } from "../types/document";

/** Un envoi de 10 Mo sur une connexion mobile dépasse le délai par défaut du client. */
const TRANSFER_TIMEOUT_MS = 120000;

const base = (vehicleId: string) => `api/vehicles/${vehicleId}/documents`;

export async function listDocuments(vehicleId: string, categoryId: string | null): Promise<VehicleDocument[]> {
  const { data } = await apiClient.get<VehicleDocument[]>(base(vehicleId), {
    params: categoryId ? { categoryId } : undefined,
  });
  return data;
}

export async function addDocument(vehicleId: string, upload: DocumentUpload): Promise<VehicleDocument> {
  const form = new FormData();
  form.append("file", upload.file);
  form.append("categoryId", upload.categoryId);
  if (upload.label.trim() !== "") form.append("label", upload.label.trim());
  const { data } = await apiClient.post<VehicleDocument>(base(vehicleId), form, { timeout: TRANSFER_TIMEOUT_MS });
  return data;
}

export async function deleteDocument(vehicleId: string, documentId: string): Promise<void> {
  await apiClient.delete(`${base(vehicleId)}/${documentId}`);
}

/** Récupère le contenu avec le jeton (client authentifié) : jamais de jeton dans une URL. */
export async function fetchDocumentContent(vehicleId: string, documentId: string): Promise<Blob> {
  const { data } = await apiClient.get<Blob>(`${base(vehicleId)}/${documentId}/content`, {
    responseType: "blob",
    timeout: TRANSFER_TIMEOUT_MS,
  });
  return data;
}
