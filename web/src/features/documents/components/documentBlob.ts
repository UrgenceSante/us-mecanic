import { fetchDocumentContent } from "../api/documentsApi";
import type { VehicleDocument } from "../types/document";

/** L'onglet doit garder l'URL le temps de charger le document avant sa révocation. */
const REVOKE_DELAY_MS = 60000;

/** Télécharge sous le nom d'origine : lien `download` sur une URL locale (blob:), révoquée aussitôt. */
export async function downloadDocument(document: VehicleDocument): Promise<void> {
  const blob = await fetchDocumentContent(document.vehicleId, document.id);
  const url = URL.createObjectURL(blob);
  const link = window.document.createElement("a");
  link.href = url;
  link.download = document.fileName;
  window.document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

/**
 * Ouvre dans un nouvel onglet. L'onglet est ouvert de façon synchrone au clic (sinon iOS le bloque après un await),
 * puis reçoit l'URL locale une fois le contenu récupéré. Si le navigateur refuse l'onglet : téléchargement.
 * À appeler directement dans le gestionnaire de clic, avant tout await.
 */
export async function openDocument(document: VehicleDocument): Promise<void> {
  const tab = window.open("", "_blank");
  if (!tab) return downloadDocument(document);
  try {
    const blob = await fetchDocumentContent(document.vehicleId, document.id);
    const url = URL.createObjectURL(blob);
    tab.location.href = url;
    setTimeout(() => URL.revokeObjectURL(url), REVOKE_DELAY_MS);
  } catch (error) {
    tab.close();
    throw error;
  }
}
