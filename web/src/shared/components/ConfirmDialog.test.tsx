import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import ConfirmDialog from "./ConfirmDialog";
import DeactivateDialog from "./DeactivateDialog";

function renderConfirm(props: Partial<React.ComponentProps<typeof ConfirmDialog>> = {}) {
  const onConfirm = vi.fn();
  const onClose = vi.fn();
  render(
    <ConfirmDialog title="Supprimer le relevé" confirmLabel="Supprimer" onConfirm={onConfirm} onClose={onClose} {...props}>
      Supprimer le relevé de 12 000 km ?
    </ConfirmDialog>,
  );
  return { onConfirm, onClose };
}

describe("ConfirmDialog", () => {
  it("affiche le titre, le message et « Cette action est définitive »", () => {
    renderConfirm();
    expect(screen.getByRole("dialog", { name: "Supprimer le relevé" })).toBeInTheDocument();
    expect(screen.getByText("Supprimer le relevé de 12 000 km ?")).toBeInTheDocument();
    expect(screen.getByText("Cette action est définitive.")).toBeInTheDocument();
  });

  it("peut omettre le texte « définitive »", () => {
    renderConfirm({ definitive: false });
    expect(screen.queryByText("Cette action est définitive.")).not.toBeInTheDocument();
  });

  it("confirme par le bouton principal, en couleur d'alerte et à droite", () => {
    const { onConfirm } = renderConfirm();
    expect(screen.getAllByRole("button").map((b) => b.textContent)).toEqual(["Annuler", "Supprimer"]);
    const confirm = screen.getByRole("button", { name: "Supprimer" });
    expect(confirm.className).toContain("colorError");
    fireEvent.click(confirm);
    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it("annule sans confirmer", () => {
    const { onConfirm, onClose } = renderConfirm();
    fireEvent.click(screen.getByRole("button", { name: "Annuler" }));
    expect(onClose).toHaveBeenCalled();
    expect(onConfirm).not.toHaveBeenCalled();
  });

  it("annonce l'erreur et désactive la confirmation pendant l'envoi", () => {
    renderConfirm({ error: "Suppression impossible.", submitting: true });
    expect(screen.getByRole("alert")).toHaveTextContent("Suppression impossible.");
    expect(screen.getByRole("button", { name: "Supprimer" })).toBeDisabled();
  });
});

describe("DeactivateDialog", () => {
  it("accorde le message au masculin", () => {
    render(<DeactivateDialog definite="le garage" name="Dupont" submitting={false} onConfirm={vi.fn()} onClose={vi.fn()} />);
    expect(screen.getByText(/Désactiver le garage « Dupont » \? Il ne sera plus proposé à la saisie\./)).toBeInTheDocument();
  });

  it("accorde le message au féminin", () => {
    render(<DeactivateDialog definite="la société" feminine name="Nord" submitting={false} onConfirm={vi.fn()} onClose={vi.fn()} />);
    expect(screen.getByText(/Elle ne sera plus proposée à la saisie\./)).toBeInTheDocument();
    expect(screen.queryByText("Cette action est définitive.")).not.toBeInTheDocument();
  });
});
