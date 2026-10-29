import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import DashboardPage from "./DashboardPage";

const api = vi.hoisted(() => ({ listDashboardDeadlines: vi.fn() }));
const vehiclesApi = vi.hoisted(() => ({ listVehicles: vi.fn() }));
const typesApi = vi.hoisted(() => ({ listDeadlineTypes: vi.fn() }));
const vehicleTypesApi = vi.hoisted(() => ({ listVehicleTypes: vi.fn() }));
const deadlinesApi = vi.hoisted(() => ({ completeVehicleDeadline: vi.fn() }));
vi.mock("../api/dashboardApi", () => api);
vi.mock("../../vehicles/api/vehiclesApi", () => vehiclesApi);
vi.mock("../../deadline-types/api/deadlineTypesApi", () => typesApi);
vi.mock("../../vehicle-types/api/vehicleTypesApi", () => vehicleTypesApi);
vi.mock("../../vehicle-deadlines/api/vehicleDeadlinesApi", () => deadlinesApi);
vi.mock("../../issues/components/DashboardProblems", () => ({ default: () => <div>Liste des problèmes</div> }));
vi.mock("../../../hooks/useNotifSnack", () => ({
  default: () => ({ notifySuccess: vi.fn(), notifyError: vi.fn() }),
}));

const rows = [
  {
    id: "d1",
    vehicle: { id: "v1", licensePlate: "AB-123-CD", internalNumber: null, typeId: null, typeName: null },
    deadlineType: { id: "t1", name: "Vidange" },
    status: "Overdue",
    dueDate: null,
    dueKm: 80000,
    currentKm: 85000,
  },
  {
    id: "d2",
    vehicle: { id: "v2", licensePlate: "EF-456-GH", internalNumber: "A12", typeId: null, typeName: null },
    deadlineType: { id: "t2", name: "Contrôle technique" },
    status: "DueSoon",
    dueDate: "2026-12-01",
    dueKm: null,
    currentKm: null,
  },
];

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <DashboardPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("DashboardPage", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listDashboardDeadlines.mockResolvedValue(rows);
    vehiclesApi.listVehicles.mockResolvedValue([{ id: "v1", licensePlate: "AB-123-CD" }]);
    typesApi.listDeadlineTypes.mockResolvedValue([{ id: "t1", name: "Vidange", isActive: true }]);
    vehicleTypesApi.listVehicleTypes.mockResolvedValue([]);
  });

  it("affiche les échéances, leur statut et un lien vers la fiche véhicule", async () => {
    renderPage();
    const link = await screen.findByRole("link", { name: "AB-123-CD" });
    expect(link).toHaveAttribute("href", "/vehicles/v1");
    expect(screen.getByText("Dépassée")).toBeInTheDocument();
    expect(screen.getByText("Proche")).toBeInTheDocument();
    expect(screen.getByText("Contrôle technique")).toBeInTheDocument();
    expect(screen.getByText("85 000 km")).toBeInTheDocument();
    expect(screen.getByText("01/12/2026")).toBeInTheDocument();
  });

  it("transmet les filtres à l'API", async () => {
    renderPage();
    await screen.findByText("Vidange");
    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Statut" }));
    fireEvent.click(within(await screen.findByRole("listbox")).getByText("Dépassée"));
    await waitFor(() =>
      expect(api.listDashboardDeadlines).toHaveBeenLastCalledWith(expect.objectContaining({ status: "Overdue" })),
    );
    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Véhicule" }));
    fireEvent.click(within(await screen.findByRole("listbox")).getByText("AB-123-CD"));
    await waitFor(() =>
      expect(api.listDashboardDeadlines).toHaveBeenLastCalledWith({
        vehicleId: "v1",
        vehicleTypeId: "",
        deadlineTypeId: "",
        status: "Overdue",
      }),
    );
  });

  it("marque une échéance réalisée avec le km obligatoire", async () => {
    deadlinesApi.completeVehicleDeadline.mockResolvedValue({});
    renderPage();
    fireEvent.click(await screen.findByRole("button", { name: "Marquer réalisée : Vidange AB-123-CD" }));
    expect(screen.getByLabelText(/Kilométrage/)).toBeRequired();
    expect(screen.getByLabelText(/Kilométrage/)).toHaveValue(85000);
    fireEvent.click(screen.getByRole("button", { name: "Enregistrer" }));
    await waitFor(() =>
      expect(deadlinesApi.completeVehicleDeadline).toHaveBeenCalledWith(
        "v1",
        "d1",
        expect.objectContaining({ kilometers: 85000 }),
      ),
    );
    await waitFor(() => expect(api.listDashboardDeadlines).toHaveBeenCalledTimes(2));
  });

  it("affiche la liste des problèmes dans l'onglet Problèmes", async () => {
    renderPage();
    await screen.findByText("Vidange");
    fireEvent.click(screen.getByRole("tab", { name: "Problèmes" }));
    expect(screen.getByText("Liste des problèmes")).toBeInTheDocument();
  });
});
