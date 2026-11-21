import { useCallback, useState } from "react";

/** Message d'erreur d'un champ, ou une valeur vide si le champ est valide. */
export type FieldCheck = string | null | false | undefined;

/** Donne le focus au champ de formulaire de ce nom (une liste déroulante reçoit le focus sur son bouton). */
export function focusField(name: string): void {
  const el = document.querySelector<HTMLElement>(`[name="${name}"]`);
  if (!el) return;
  const target =
    el.getAttribute("aria-hidden") === "true" ? el.parentElement?.querySelector<HTMLElement>("[role=combobox], [role=spinbutton]") : el;
  (target ?? el).focus();
}

/**
 * Validation commune des formulaires : messages français sous chaque champ, `aria-invalid` posé par MUI,
 * focus sur le premier champ en erreur (dans l'ordre des clés passées à `validate`).
 */
export default function useFormErrors<F extends string>() {
  const [errors, setErrors] = useState<Partial<Record<F, string>>>({});

  /** Évalue les champs ; renvoie vrai s'ils sont tous valides, sinon affiche les erreurs et focalise la première. */
  const validate = useCallback((checks: Partial<Record<F, FieldCheck>>): boolean => {
    const next: Partial<Record<F, string>> = {};
    for (const [name, message] of Object.entries(checks) as [F, FieldCheck][]) {
      if (message) next[name] = message;
    }
    setErrors(next);
    const first = Object.keys(next)[0];
    if (first) focusField(first);
    return first === undefined;
  }, []);

  /** Efface l'erreur d'un champ dès qu'il est modifié. */
  const clear = useCallback((...names: F[]) => {
    setErrors((current) => {
      if (!names.some((n) => current[n])) return current;
      const next = { ...current };
      for (const n of names) delete next[n];
      return next;
    });
  }, []);

  /** Props à étaler sur un champ (TextField, FrenchDateField…) ; `hint` est l'aide affichée tant qu'il n'y a pas d'erreur. */
  const fieldProps = (name: F, hint?: string) => ({
    name,
    error: errors[name] !== undefined,
    helperText: errors[name] ?? hint,
  });

  return { errors, validate, clear, fieldProps };
}
