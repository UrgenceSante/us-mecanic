import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ProblemReferencesPage from "./ProblemReferencesPage";

const api = vi.hoisted(() => ({
  listProblemReferences: vi.fn(),
  createProblemReference: vi.fn(),
  renameProblemReference: vi.fn(),
  deactivateProblemReference: vi.fn(),
}));
vi.mock("../api/problemReferencesApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <ProblemReferencesPage />
    </QueryClientProvider>,
  );
}

describe("ProblemReferencesPage", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listProblemReferences.mockResolvedValue([{ id: "n1", name: "Panne", isActive: true }]);
  });

  it("masque les inactifs par défaut et les affiche sur option", async () => {
    renderPage();
    expect(await screen.findByText("Panne")).toBeInTheDocument();
    expect(api.listProblemReferences).toHaveBeenCalledWith("api/problem-natures", false);

    fireEvent.click(screen.getByRole("checkbox"));
    await waitFor(() => expect(api.listProblemReferences).toHaveBeenCalledWith("api/problem-natures", true));
  });

  it("change de référentiel via les onglets", async () => {
    renderPage();
    await screen.findByText("Panne");

    fireEvent.click(screen.getByRole("tab", { name: "Types d'action" }));

    await waitFor(() => expect(api.listProblemReferences).toHaveBeenCalledWith("api/problem-action-types", false));
  });

  it("crée une valeur", async () => {
    api.createProblemReference.mockResolvedValue({ id: "n2", name: "Casse", isActive: true });
    renderPage();
    await screen.findByText("Panne");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: " Casse " } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.createProblemReference).toHaveBeenCalledWith("api/problem-natures", "Casse"));
  });

  it("renomme et affiche l'erreur métier traduite", async () => {
    api.renameProblemReference.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { title: "Conflict", code: "problem_nature.name_already_exists" } },
    });
    renderPage();
    await screen.findByText("Panne");

    fireEvent.click(screen.getByRole("button", { name: "Renommer Panne" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: "Casse" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Une nature portant ce nom existe déjà.")).toBeInTheDocument();
    expect(api.renameProblemReference).toHaveBeenCalledWith("api/problem-natures", "n1", "Casse");
  });

  it("désactive une valeur", async () => {
    api.deactivateProblemReference.mockResolvedValue({ id: "n1", name: "Panne", isActive: false });
    renderPage();
    await screen.findByText("Panne");

    fireEvent.click(screen.getByRole("button", { name: "Désactiver Panne" }));

    await waitFor(() => expect(api.deactivateProblemReference).toHaveBeenCalledWith("api/problem-natures", "n1"));
  });
});
