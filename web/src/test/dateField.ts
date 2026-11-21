import { fireEvent, screen } from "@testing-library/react";

/** `2026-05-01` -> `01/05/2026`, tel qu'affiché par `FrenchDateField`. */
export const frenchDate = (iso: string): string => iso.split("-").reverse().join("/");

/** Champ masqué qui porte la valeur d'un `FrenchDateField` (le seul `input` associé au libellé). */
export function dateInput(label: string | RegExp): HTMLInputElement {
  const input = screen.getAllByLabelText(label).find((el): el is HTMLInputElement => el.tagName === "INPUT");
  if (!input) throw new Error(`Aucun champ date pour ${String(label)}`);
  return input;
}

/** Saisit une date ISO dans un `FrenchDateField`. */
export function setDate(label: string | RegExp, iso: string): void {
  fireEvent.change(dateInput(label), { target: { value: frenchDate(iso) } });
}
