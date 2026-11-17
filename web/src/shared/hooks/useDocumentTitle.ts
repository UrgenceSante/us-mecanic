import { useEffect } from "react";

export const appName = "Urgence Santé";

/** Format du titre d'onglet : « <page> — Urgence Santé ». */
export const formatDocumentTitle = (page: string) => `${page} — ${appName}`;

/** Fixe `document.title` pour la page affichée (WCAG 2.4.2) et le restaure au démontage. */
export default function useDocumentTitle(page: string) {
  useEffect(() => {
    const previous = document.title;
    document.title = formatDocumentTitle(page);
    return () => {
      document.title = previous;
    };
  }, [page]);
}
