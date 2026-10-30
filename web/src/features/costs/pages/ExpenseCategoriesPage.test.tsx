import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ExpenseCategoriesPage from "./ExpenseCategoriesPage";

const api = vi.hoisted(() => ({
  listExpenseCategories: vi.fn(),
  createExpenseCategory: vi.fn(),
  renameExpenseCategory: vi.fn(),
  deactivateExpenseCategory: vi.fn(),
}));
vi.mock("../api/expenseCategoriesApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <ExpenseCategoriesPage />
    </QueryClientProvider>,
  );
}

function apiError(status: number, code: string) {
  return { isAxiosError: true, response: { status, data: { title: "Erreur", code } } };
}

describe("ExpenseCategoriesPage", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listExpenseCategories.mockResolvedValue([
      { id: "c1", name: "Carburant", isActive: true },
      { id: "c2", name: "Ancienne", isActive: false },
    ]);
  });

  it("liste les catégories actives et désactivées, sans bouton de désactivation pour les désactivées", async () => {
    renderPage();

    expect(screen.getByLabelText("Chargement")).toBeInTheDocument();
    expect(await screen.findByText("Carburant")).toBeInTheDocument();
    expect(api.listExpenseCategories).toHaveBeenCalledWith(true);
    expect(screen.getByText("Désactivé")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Désactiver Carburant" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Ancienne" })).not.toBeInTheDocument();
  });

  it("crée une catégorie", async () => {
    api.createExpenseCategory.mockResolvedValue({ id: "c3", name: "Péage", isActive: true });
    renderPage();
    await screen.findByText("Carburant");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: " Péage " } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.createExpenseCategory).toHaveBeenCalledWith("Péage"));
  });

  it("affiche l'erreur de nom vide renvoyée par l'API", async () => {
    api.createExpenseCategory.mockRejectedValue(apiError(400, "expense_category.name_required"));
    renderPage();
    await screen.findByText("Carburant");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Le nom de la catégorie est obligatoire.")).toBeInTheDocument();
  });

  it("renomme une catégorie et affiche l'erreur de doublon en français", async () => {
    api.renameExpenseCategory.mockRejectedValue(apiError(409, "expense_category.name_already_exists"));
    renderPage();
    await screen.findByText("Carburant");

    fireEvent.click(screen.getByRole("button", { name: "Renommer Carburant" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: "Péage" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Une catégorie portant ce nom existe déjà.")).toBeInTheDocument();
    expect(api.renameExpenseCategory).toHaveBeenCalledWith("c1", "Péage");
  });

  it("désactive une catégorie après confirmation", async () => {
    api.deactivateExpenseCategory.mockResolvedValue({ id: "c1", name: "Carburant", isActive: false });
    renderPage();
    await screen.findByText("Carburant");

    fireEvent.click(screen.getByRole("button", { name: "Désactiver Carburant" }));
    expect(api.deactivateExpenseCategory).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole("button", { name: "Confirmer la désactivation" }));

    await waitFor(() => expect(api.deactivateExpenseCategory).toHaveBeenCalledWith("c1"));
  });
});
