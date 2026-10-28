import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import DeadlineTypesPage from "./DeadlineTypesPage";

const api = vi.hoisted(() => ({
  listDeadlineTypes: vi.fn(),
  createDeadlineType: vi.fn(),
  updateDeadlineType: vi.fn(),
  deactivateDeadlineType: vi.fn(),
  getDeadlineThresholds: vi.fn(),
  updateDeadlineThresholds: vi.fn(),
}));
vi.mock("../api/deadlineTypesApi", () => api);
vi.mock("../../vehicle-types/api/vehicleTypesApi", () => ({ listVehicleTypes: vi.fn().mockResolvedValue([]) }));
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

const ars = {
  id: "t1",
  name: "Agrément ARS",
  defaultIntervalMonths: 12,
  defaultIntervalKm: 30000,
  dueSoonDays: 15,
  dueSoonKm: null,
  isOneOff: false,
  isActive: true,
};
const oneOff = {
  id: "t2",
  name: "Contrôle initial",
  defaultIntervalMonths: null,
  defaultIntervalKm: null,
  dueSoonDays: null,
  dueSoonKm: null,
  isOneOff: true,
  isActive: false,
};

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <DeadlineTypesPage />
    </QueryClientProvider>,
  );
}

describe("DeadlineTypesPage", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listDeadlineTypes.mockResolvedValue([ars, oneOff]);
    api.getDeadlineThresholds.mockResolvedValue({ dueSoonDays: 30, dueSoonKm: 2000 });
  });

  it("affiche périodicité, seuils propres ou globaux, et statut", async () => {
    renderPage();

    expect(await screen.findByText("Agrément ARS")).toBeInTheDocument();
    expect(api.listDeadlineTypes).toHaveBeenCalledWith(true);
    expect(screen.getByText(/12 mois \/ 30\s000 km/)).toBeInTheDocument();
    expect(screen.getByText("15 j")).toBeInTheDocument();
    expect(screen.getByText("Ponctuel")).toBeInTheDocument();
    expect(screen.getByText("seuils globaux")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Désactiver Agrément ARS" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Désactiver Contrôle initial" })).not.toBeInTheDocument();
  });

  it("crée un type ponctuel avec les champs facultatifs à null", async () => {
    api.createDeadlineType.mockResolvedValue(ars);
    renderPage();
    await screen.findByText("Agrément ARS");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), { target: { value: " Extincteur " } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() =>
      expect(api.createDeadlineType).toHaveBeenCalledWith({
        name: "Extincteur",
        defaultIntervalMonths: null,
        defaultIntervalKm: null,
        dueSoonDays: null,
        dueSoonKm: null,
      }),
    );
  });

  it("modifie un type en renvoyant tous les champs et affiche l'erreur métier", async () => {
    api.updateDeadlineType.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409, data: { title: "Conflict", code: "deadline_type.name_already_exists" } },
    });
    renderPage();
    await screen.findByText("Agrément ARS");

    fireEvent.click(screen.getByRole("button", { name: "Modifier Agrément ARS" }));
    fireEvent.change(screen.getByLabelText("Périodicité (mois)"), { target: { value: "24" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(await screen.findByText("Un type d'échéance portant ce nom existe déjà.")).toBeInTheDocument();
    expect(api.updateDeadlineType).toHaveBeenCalledWith("t1", {
      name: "Agrément ARS",
      defaultIntervalMonths: 24,
      defaultIntervalKm: 30000,
      dueSoonDays: 15,
      dueSoonKm: null,
    });
  });

  it("désactive un type", async () => {
    api.deactivateDeadlineType.mockResolvedValue({ ...ars, isActive: false });
    renderPage();
    await screen.findByText("Agrément ARS");

    fireEvent.click(screen.getByRole("button", { name: "Désactiver Agrément ARS" }));

    await waitFor(() => expect(api.deactivateDeadlineType).toHaveBeenCalledWith("t1"));
  });

  it("modifie les seuils globaux", async () => {
    api.updateDeadlineThresholds.mockResolvedValue({ dueSoonDays: 45, dueSoonKm: 2000 });
    renderPage();

    const days = await screen.findByLabelText(/Jours avant échéance/);
    expect(days).toHaveValue(30);
    fireEvent.change(days, { target: { value: "45" } });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer les seuils" }));

    await waitFor(() =>
      expect(api.updateDeadlineThresholds).toHaveBeenCalledWith({ dueSoonDays: 45, dueSoonKm: 2000 }),
    );
  });
});
