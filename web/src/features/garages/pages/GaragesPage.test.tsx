import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import GaragesPage from "./GaragesPage";

const api = vi.hoisted(() => ({
  listGarages: vi.fn(),
  createGarage: vi.fn(),
  renameGarage: vi.fn(),
  deactivateGarage: vi.fn(),
}));
vi.mock("../api/garagesApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <GaragesPage />
    </QueryClientProvider>,
  );
}

describe("GaragesPage", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listGarages.mockResolvedValue([
      { id: "g1", name: "Garage Dupont", isActive: true },
      { id: "g2", name: "Ancien garage", isActive: false },
    ]);
  });

  it("liste les garages actifs et désactivés, avec la désactivation réservée aux actifs", async () => {
    renderPage();

    expect(await screen.findByText("Garage Dupont")).toBeInTheDocument();
    expect(api.listGarages).toHaveBeenCalledWith(true);
    expect(screen.getByText("Désactivé")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Désactiver Garage Dupont" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Ancien garage" })).not.toBeInTheDocument();
  });

  it("crée un garage", async () => {
    api.createGarage.mockResolvedValue({ id: "g3", name: "Norauto", isActive: true });
    renderPage();
    await screen.findByText("Garage Dupont");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: " Norauto " } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.createGarage).toHaveBeenCalledWith("Norauto"));
  });

  it("renomme un garage et affiche l'erreur métier en français", async () => {
    api.renameGarage.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { title: "Conflict", code: "garage.name_already_exists" } },
    });
    renderPage();
    await screen.findByText("Garage Dupont");

    fireEvent.click(screen.getByRole("button", { name: "Renommer Garage Dupont" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: "Norauto" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Un garage portant ce nom existe déjà.")).toBeInTheDocument();
    expect(api.renameGarage).toHaveBeenCalledWith("g1", "Norauto");
  });

  it("désactive un garage", async () => {
    api.deactivateGarage.mockResolvedValue({ id: "g1", name: "Garage Dupont", isActive: false });
    renderPage();
    await screen.findByText("Garage Dupont");

    fireEvent.click(screen.getByRole("button", { name: "Désactiver Garage Dupont" }));

    await waitFor(() => expect(api.deactivateGarage).toHaveBeenCalledWith("g1"));
  });
});
