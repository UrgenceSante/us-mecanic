export interface VehicleDocument {
  id: string;
  vehicleId: string;
  categoryId: string | null;
  interventionId: string | null;
  expenseId: string | null;
  label: string | null;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  addedAt: string;
}

export interface DocumentUpload {
  file: File;
  categoryId: string;
  label: string;
}

/** Nom affiché : libellé, à défaut nom du fichier d'origine. */
export function documentDisplayName(document: VehicleDocument): string {
  return document.label?.trim() || document.fileName;
}
