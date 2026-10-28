import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";
import DeadlineTypesPage from "./DeadlineTypesPage";

const api = vi.hoisted(() => ({
  listDeadlineTypes: vi.fn(),
  createDeadlineType: vi.fn(),
  updateDeadlineType: vi.fn(),
  deactivateDeadlineType: vi.fn(),
  updateDeadlineThresholds: vi.fn(),
  getDeadlineThresholds: vi.fn(),
  assignDeadlineType: vi.fn(),
}));
const vehicleTypesApi = vi.hoisted(() => ({ listVehicleTypes: vi.fn() }));
vi.mock("../api/deadlineTypesApi", () => api);
vi.mock("../../vehicle-types/api/vehicleTypesApi", () => vehicleTypesApi);
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
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <DeadlineTypesPage />
    </QueryClientProvider>,
  );
}

describe("DeadlineTypesPage : ajout à des véhicules", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listDeadlineTypes.mockResolvedValue([ars, oneOff]);
    api.getDeadlineThresholds.mockResolvedValue({
      dueSoonDays: 30,
      dueSoonKm: 2000,
    });
    vehicleTypesApi.listVehicleTypes.mockResolvedValue([
      { id: "vt1", name: "Ambulance", isActive: true },
      { id: "vt2", name: "VSL", isActive: true },
    ]);
  });

  it("propose l'ajout aux véhicules après la création d'un type périodique et affiche le bilan", async () => {
    api.createDeadlineType.mockResolvedValue(ars);
    api.assignDeadlineType.mockResolvedValue({
      added: 3,
      skippedAlreadyActive: 1,
      skippedRetired: 2,
      errors: [],
    });
    renderPage();
    await screen.findByText("Agrément ARS");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), {
      target: { value: "Agrément ARS" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    expect(
      await screen.findByText(/Ajouter « Agrément ARS » à des véhicules/),
    ).toBeInTheDocument();
    const submit = screen.getByRole("button", { name: "Ajouter" });
    expect(submit).toBeDisabled();
    fireEvent.click(await screen.findByLabelText("Ambulance"));
    fireEvent.click(screen.getByLabelText("VSL"));
    fireEvent.change(screen.getByLabelText("Dernière réalisation"), {
      target: { value: "2026-09-01" },
    });
    fireEvent.click(submit);

    await waitFor(() =>
      expect(api.assignDeadlineType).toHaveBeenCalledWith("t1", {
        vehicleTypeIds: ["vt1", "vt2"],
        lastDoneDate: "2026-09-01",
      }),
    );
    expect(
      await screen.findByText(/3 échéance\(s\) ajoutée\(s\)/),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/1 déjà concerné\(s\), 2 sorti\(s\) du parc/),
    ).toBeInTheDocument();
  });

  it("ne propose pas l'ajout après la création d'un type ponctuel", async () => {
    api.createDeadlineType.mockResolvedValue({ ...oneOff, isActive: true });
    renderPage();
    await screen.findByText("Agrément ARS");

    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));
    fireEvent.change(screen.getByLabelText(/Nom/), {
      target: { value: "Contrôle initial" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));

    await waitFor(() => expect(api.createDeadlineType).toHaveBeenCalled());
    await waitFor(() =>
      expect(
        screen.queryByRole("button", { name: "Enregistrer" }),
      ).not.toBeInTheDocument(),
    );
    expect(screen.queryByText(/à des véhicules/)).not.toBeInTheDocument();
  });

  it("propose l'ajout depuis un type actif périodique, sans date, et affiche les véhicules en erreur", async () => {
    api.assignDeadlineType.mockResolvedValue({
      added: 0,
      skippedAlreadyActive: 0,
      skippedRetired: 0,
      errors: [
        {
          vehicleId: "v1",
          licensePlate: "AB-123-CD",
          code: "x",
          message: "Kilométrage manquant",
        },
      ],
    });
    renderPage();
    await screen.findByText("Agrément ARS");
    expect(
      screen.queryByRole("button", {
        name: "Ajouter Contrôle initial à des véhicules",
      }),
    ).not.toBeInTheDocument();

    fireEvent.click(
      screen.getByRole("button", {
        name: "Ajouter Agrément ARS à des véhicules",
      }),
    );
    fireEvent.click(await screen.findByLabelText("Ambulance"));
    fireEvent.click(screen.getByRole("button", { name: "Ajouter" }));

    await waitFor(() =>
      expect(api.assignDeadlineType).toHaveBeenCalledWith("t1", {
        vehicleTypeIds: ["vt1"],
        lastDoneDate: null,
      }),
    );
    expect(await screen.findByText("AB-123-CD")).toBeInTheDocument();
    expect(screen.getByText("Kilométrage manquant")).toBeInTheDocument();
  });
});
