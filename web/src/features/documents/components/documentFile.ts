export const MAX_DOCUMENT_BYTES = 10 * 1024 * 1024;
export const ACCEPTED_TYPES = "application/pdf,image/jpeg,image/png";
const accepted = ACCEPTED_TYPES.split(",");

export const TYPE_ERROR = "Format non accepté : PDF, JPEG ou PNG uniquement.";
export const SIZE_ERROR = "Fichier trop volumineux (10 Mo maximum).";
export const EMPTY_ERROR = "Le fichier est vide.";

/** Contrôle avant envoi (l'API recontrôle sur le contenu). Renvoie le message d'erreur, ou null si le fichier convient. */
export function validateDocumentFile(file: File): string | null {
  if (!accepted.includes(file.type)) return TYPE_ERROR;
  if (file.size === 0) return EMPTY_ERROR;
  if (file.size > MAX_DOCUMENT_BYTES) return SIZE_ERROR;
  return null;
}

/** Taille lisible en Ko ou Mo. */
export function formatFileSize(bytes: number): string {
  const nf = (value: number) => value.toLocaleString("fr-FR", { maximumFractionDigits: 1 });
  if (bytes < 1024 * 1024) return `${nf(Math.max(bytes, 1) / 1024)} Ko`;
  return `${nf(bytes / (1024 * 1024))} Mo`;
}
