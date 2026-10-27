import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import VehicleTypesPage from "./VehicleTypesPage";

const api = vi.hoisted(() => ({
  listVehicleTypes: vi.fn(),
  createVehicleType: vi.fn(),
  renameVehicleType: vi.fn(),
  deactivateVehicleType: vi.fn(),
}));
vi.mock("../api/vehicleTypesApi", () => api);
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <VehicleTypesPage />
    </QueryClientProvider>,
  );
}

describe("VehicleTypesPage", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listVehicleTypes.mockResolvedValue([
      { id: "t1", name: "Ambulance", isActive: true },
      { id: "t2", name: "Sang", isActive: false },
    ]);
  });

  it("liste les types actifs et désactivés, avec la désactivation réservée aux actifs", async () => {
    renderPage();

    expect(await screen.findByText("Ambulance")).toBeInTheDocument();
    expect(api.listVehicleTypes).toHaveBeenCalledWith(true);
    expect(screen.getByText("Désactivé")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Désactiver Ambulance" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Sang" })).not.toBeInTheDocument();
  });

  it("crée un type", async () => {
    api.createVehicleType.mockResolvedValue({ id: "t3", name: "VSL", isActive: true });
    renderPage();
    await screen.findByText("Ambulance");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: " VSL " } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.createVehicleType).toHaveBeenCalledWith("VSL"));
  });

  it("renomme un type et affiche l'erreur métier en français", async () => {
    api.renameVehicleType.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { title: "Conflict", code: "vehicle_type.name_already_exists" } },
    });
    renderPage();
    await screen.findByText("Ambulance");

    fireEvent.click(screen.getByRole("button", { name: "Renommer Ambulance" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: "VSL" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Un type portant ce nom existe déjà.")).toBeInTheDocument();
    expect(api.renameVehicleType).toHaveBeenCalledWith("t1", "VSL");
  });

  it("désactive un type", async () => {
    api.deactivateVehicleType.mockResolvedValue({ id: "t1", name: "Ambulance", isActive: false });
    renderPage();
    await screen.findByText("Ambulance");

    fireEvent.click(screen.getByRole("button", { name: "Désactiver Ambulance" }));

    await waitFor(() => expect(api.deactivateVehicleType).toHaveBeenCalledWith("t1"));
  });
});
