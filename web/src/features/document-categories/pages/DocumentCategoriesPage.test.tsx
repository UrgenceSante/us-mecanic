import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import DocumentCategoriesPage from "./DocumentCategoriesPage";

const api = vi.hoisted(() => ({
  listDocumentCategories: vi.fn(),
  createDocumentCategory: vi.fn(),
  renameDocumentCategory: vi.fn(),
  deactivateDocumentCategory: vi.fn(),
}));
vi.mock("../api/documentCategoriesApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <DocumentCategoriesPage />
    </QueryClientProvider>,
  );
}

function mockMatchMedia(matches: boolean) {
  vi.stubGlobal("matchMedia", (query: string) => ({
    matches,
    media: query,
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    addListener: vi.fn(),
    removeListener: vi.fn(),
  }));
}

describe("DocumentCategoriesPage", () => {
  afterEach(() => vi.unstubAllGlobals());

  beforeEach(() => {
    vi.resetAllMocks();
    api.listDocumentCategories.mockResolvedValue([
      { id: "c1", name: "Carte grise", isActive: true },
      { id: "c2", name: "Ancienne catégorie", isActive: false },
    ]);
  });

  it("affiche le bandeau d'information", async () => {
    renderPage();

    expect(
      await screen.findByText(
        "Une catégorie désactivée n'est plus proposée à l'ajout d'un document, mais reste affichée sur les documents déjà classés.",
      ),
    ).toBeInTheDocument();
  });

  it("affiche un indicateur pendant le chargement", () => {
    api.listDocumentCategories.mockReturnValue(new Promise(() => {}));
    renderPage();

    expect(screen.getByLabelText("Chargement")).toBeInTheDocument();
  });

  it("liste les catégories actives et désactivées, avec la désactivation réservée aux actives", async () => {
    renderPage();

    expect(await screen.findByText("Carte grise")).toBeInTheDocument();
    expect(api.listDocumentCategories).toHaveBeenCalledWith(true);
    expect(screen.getByText("Désactivée")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Désactiver Carte grise" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Ancienne catégorie" })).not.toBeInTheDocument();
  });

  it("crée une catégorie", async () => {
    api.createDocumentCategory.mockResolvedValue({ id: "c3", name: "Photo", isActive: true });
    renderPage();
    await screen.findByText("Carte grise");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: " Photo " } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.createDocumentCategory).toHaveBeenCalledWith("Photo"));
  });

  it("renomme une catégorie et affiche le doublon en français sous le champ", async () => {
    api.renameDocumentCategory.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { title: "Conflict", code: "document_category.name_already_exists" } },
    });
    renderPage();
    await screen.findByText("Carte grise");

    fireEvent.click(screen.getByRole("button", { name: "Renommer Carte grise" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: "Photo" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Une catégorie porte déjà ce nom.")).toBeInTheDocument();
    expect(api.renameDocumentCategory).toHaveBeenCalledWith("c1", "Photo");
  });

  it("désactive une catégorie", async () => {
    api.deactivateDocumentCategory.mockResolvedValue({ id: "c1", name: "Carte grise", isActive: false });
    renderPage();
    await screen.findByText("Carte grise");

    fireEvent.click(screen.getByRole("button", { name: "Désactiver Carte grise" }));
    expect(api.deactivateDocumentCategory).not.toHaveBeenCalled();
    fireEvent.click(await screen.findByRole("button", { name: "Désactiver" }));

    await waitFor(() => expect(api.deactivateDocumentCategory).toHaveBeenCalledWith("c1"));
  });

  it("affiche des cartes sur téléphone, avec les mêmes actions", async () => {
    mockMatchMedia(true);
    renderPage();

    expect(await screen.findByRole("list", { name: "Catégories de document" })).toBeInTheDocument();
    expect(await screen.findAllByTestId("list-card")).toHaveLength(2);
    expect(screen.getByRole("button", { name: "Renommer Carte grise" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Ancienne catégorie" })).not.toBeInTheDocument();
  });

  it("affiche un message quand il n'y a aucune catégorie", async () => {
    api.listDocumentCategories.mockResolvedValue([]);
    renderPage();

    expect(await screen.findByText("Aucune catégorie.")).toBeInTheDocument();
  });
});
