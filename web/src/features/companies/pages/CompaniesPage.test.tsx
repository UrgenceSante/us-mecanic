import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import CompaniesPage from "./CompaniesPage";

const api = vi.hoisted(() => ({
  listCompanies: vi.fn(),
  createCompany: vi.fn(),
  renameCompany: vi.fn(),
  deactivateCompany: vi.fn(),
}));
vi.mock("../api/companiesApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <CompaniesPage />
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

describe("CompaniesPage", () => {
  afterEach(() => vi.unstubAllGlobals());

  beforeEach(() => {
    vi.resetAllMocks();
    api.listCompanies.mockResolvedValue([
      { id: "g1", name: "Ambulances Nord", isActive: true },
      { id: "g2", name: "Ancienne société", isActive: false },
    ]);
  });

  it("liste les sociétés actives et désactivées, avec la désactivation réservée aux actifs", async () => {
    renderPage();

    expect(await screen.findByText("Ambulances Nord")).toBeInTheDocument();
    expect(api.listCompanies).toHaveBeenCalledWith(true);
    expect(screen.getByText("Désactivée")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Désactiver Ambulances Nord" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Ancienne société" })).not.toBeInTheDocument();
  });

  it("crée une société", async () => {
    api.createCompany.mockResolvedValue({ id: "g3", name: "Pompes funèbres Sud", isActive: true });
    renderPage();
    await screen.findByText("Ambulances Nord");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: " Pompes funèbres Sud " } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.createCompany).toHaveBeenCalledWith("Pompes funèbres Sud"));
  });

  it("renomme une société et affiche l'erreur métier en français", async () => {
    api.renameCompany.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { title: "Conflict", code: "company.name_already_exists" } },
    });
    renderPage();
    await screen.findByText("Ambulances Nord");

    fireEvent.click(screen.getByRole("button", { name: "Renommer Ambulances Nord" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: "Pompes funèbres Sud" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Une société porte déjà ce nom.")).toBeInTheDocument();
    expect(api.renameCompany).toHaveBeenCalledWith("g1", "Pompes funèbres Sud");
  });

  it("désactive une société", async () => {
    api.deactivateCompany.mockResolvedValue({ id: "g1", name: "Ambulances Nord", isActive: false });
    renderPage();
    await screen.findByText("Ambulances Nord");

    fireEvent.click(screen.getByRole("button", { name: "Désactiver Ambulances Nord" }));
    expect(api.deactivateCompany).not.toHaveBeenCalled();
    fireEvent.click(await screen.findByRole("button", { name: "Désactiver" }));

    await waitFor(() => expect(api.deactivateCompany).toHaveBeenCalledWith("g1"));
  });

  it("affiche des cartes sur téléphone, avec les mêmes actions", async () => {
    mockMatchMedia(true);
    renderPage();

    expect(await screen.findByRole("list", { name: "Sociétés" })).toBeInTheDocument();
    expect(await screen.findAllByTestId("list-card")).toHaveLength(2);
    expect(screen.getByRole("button", { name: "Renommer Ambulances Nord" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Ancienne société" })).not.toBeInTheDocument();
  });

  it("affiche un message quand il n'y a aucune société", async () => {
    api.listCompanies.mockResolvedValue([]);
    renderPage();

    expect(await screen.findByText("Aucune société.")).toBeInTheDocument();
  });
});
