/** Plafond accepté par l'API (numeric(12,2)). */
export const MAX_AMOUNT = 9_999_999_999.99;

export interface ParsedAmount {
  /** null si le champ est vide ou invalide. */
  value: number | null;
  /** Message français, null si la saisie est vide ou valide. */
  error: string | null;
}

/** Interprète une saisie de montant en euros : virgule ou point, espaces ignorés, 2 décimales max, jamais négatif. */
export function parseAmount(input: string): ParsedAmount {
  const text = input.replace(/[\s\u00a0\u202f]/g, "").replace(",", ".");
  if (text === "") return { value: null, error: null };
  if (text.startsWith("-")) return { value: null, error: "Le montant ne peut pas être négatif." };
  if (!/^\d*\.?\d*$/.test(text) || !/\d/.test(text)) return { value: null, error: "Montant invalide." };
  if ((text.split(".")[1] ?? "").length > 2) return { value: null, error: "Le montant est limité à 2 décimales." };
  const value = Number(text);
  if (value > MAX_AMOUNT) return { value: null, error: "Le montant dépasse la valeur maximale autorisée." };
  return { value, error: null };
}
