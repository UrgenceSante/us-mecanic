import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { DocumentUpload } from "../types/document";
import { addDocument, deleteDocument, listDocuments } from "./documentsApi";

export const documentKeys = {
  list: (vehicleId: string, categoryId: string | null) => ["documents", vehicleId, { categoryId }] as const,
  vehicle: (vehicleId: string) => ["documents", vehicleId] as const,
};

export function useDocuments(vehicleId: string, categoryId: string | null) {
  return useQuery({
    queryKey: documentKeys.list(vehicleId, categoryId),
    queryFn: () => listDocuments(vehicleId, categoryId),
    enabled: vehicleId !== "",
  });
}

function useInvalidateDocuments(vehicleId: string) {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: documentKeys.vehicle(vehicleId) });
}

export function useAddDocument(vehicleId: string) {
  const onSuccess = useInvalidateDocuments(vehicleId);
  return useMutation({ mutationFn: (upload: DocumentUpload) => addDocument(vehicleId, upload), onSuccess });
}

export function useDeleteDocument(vehicleId: string) {
  const onSuccess = useInvalidateDocuments(vehicleId);
  return useMutation({ mutationFn: (documentId: string) => deleteDocument(vehicleId, documentId), onSuccess });
}
