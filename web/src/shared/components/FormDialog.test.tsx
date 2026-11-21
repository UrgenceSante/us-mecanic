import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import FormDialog from "./FormDialog";

function renderDialog(props: Partial<React.ComponentProps<typeof FormDialog>> = {}) {
  const onSubmit = vi.fn();
  const onClose = vi.fn();
  render(
    <FormDialog title="Ajouter une dépense" submitLabel="Ajouter" onSubmit={onSubmit} onClose={onClose} {...props}>
      <label>
        Montant
        <input name="amount" required />
      </label>
    </FormDialog>,
  );
  return { onSubmit, onClose };
}

describe("FormDialog", () => {
  it("relie le titre au dialogue", () => {
    renderDialog();
    expect(screen.getByRole("dialog", { name: "Ajouter une dépense" })).toBeInTheDocument();
  });

  it("n'utilise pas la validation native du navigateur", () => {
    renderDialog();
    const form = screen.getByRole("dialog").querySelector("form");
    expect(form).toHaveAttribute("novalidate");
  });

  it("soumet même si un champ requis est vide (la validation est faite par l'appelant)", () => {
    const { onSubmit } = renderDialog();
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    expect(onSubmit).toHaveBeenCalledTimes(1);
  });

  it("place l'action principale à droite, après Annuler", () => {
    renderDialog();
    const buttons = screen.getAllByRole("button").map((b) => b.textContent);
    expect(buttons).toEqual(["Annuler", "Ajouter"]);
  });

  it("ferme avec Annuler", () => {
    const { onClose } = renderDialog();
    fireEvent.click(screen.getByRole("button", { name: "Annuler" }));
    expect(onClose).toHaveBeenCalled();
  });

  it("annonce l'erreur globale dans une alerte", () => {
    renderDialog({ error: "Impossible d'enregistrer." });
    expect(screen.getByRole("alert")).toHaveTextContent("Impossible d'enregistrer.");
  });

  it("désactive l'action pendant l'envoi", () => {
    renderDialog({ submitting: true });
    expect(screen.getByRole("button", { name: "Ajouter" })).toBeDisabled();
  });

  it("donne le focus au premier champ", async () => {
    renderDialog();
    await waitFor(() => expect(screen.getByLabelText("Montant")).toHaveFocus());
  });

  it("n'affiche que Fermer une fois l'opération terminée", () => {
    renderDialog({ done: true });
    expect(screen.getAllByRole("button").map((b) => b.textContent)).toEqual(["Fermer"]);
  });
});
