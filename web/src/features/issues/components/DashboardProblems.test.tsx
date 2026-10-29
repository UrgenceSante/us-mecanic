import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import DashboardProblems from "./DashboardProblems";

const api = vi.hoisted(() => ({ listFleetProblems: vi.fn() }));
const vehiclesApi = vi.hoisted(() => ({ listVehicles: vi.fn() }));
const referencesApi = vi.hoisted(() => ({ listProblemReferences: vi.fn() }));
vi.mock("../api/problemsApi", () => api);
vi.mock("../../vehicles/api/vehiclesApi", () => vehiclesApi);
vi.mock("../../problem-references/api/problemReferencesApi", () => referencesApi);

const problem = (id: string, description: string, status: string) => ({
  id,
  vehicleId: "v1",
  date: "2026-03-04",
  description,
  author: "Jean",
  status,
  recordedAt: "2026-03-04T08:00:00Z",
  analysis: null,
  actions: [],
});

const rows = [
  {
    problem: problem("p1", "Freins HS", "Analyzed"),
    vehicle: { id: "v1", licensePlate: "AB-123-CD" },
    nature: { id: "n1", name: "Mécanique" },
    blockingLevel: "Immobilizing",
  },
  {
    problem: problem("p2", "Rétroviseur cassé", "Reported"),
    vehicle: { id: "v2", licensePlate: "EF-456-GH" },
    nature: null,
    blockingLevel: "Unanalyzed",
  },
  {
    problem: problem("p3", "Rayure", "Analyzed"),
    vehicle: { id: "v1", licensePlate: "AB-123-CD" },
    nature: { id: "n1", name: "Mécanique" },
    blockingLevel: "Other",
  },
];

function renderView() {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <DashboardProblems />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("DashboardProblems", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    api.listFleetProblems.mockResolvedValue(rows);
    vehiclesApi.listVehicles.mockResolvedValue([{ id: "v1", licensePlate: "AB-123-CD" }]);
    referencesApi.listProblemReferences.mockResolvedValue([{ id: "n1", name: "Mécanique", isActive: true }]);
  });

  it("affiche libellés FR, lien véhicule et nature", async () => {
    renderView();
    expect(await screen.findByText("Freins HS")).toBeInTheDocument();
    expect(screen.getByText("Immobilisant")).toBeInTheDocument();
    expect(screen.getByText("Non analysé")).toBeInTheDocument();
    expect(screen.getByText("Autre")).toBeInTheDocument();
    expect(screen.getAllByText("Analysé")).toHaveLength(2);
    expect(screen.getByText("Signalé")).toBeInTheDocument();
    expect(screen.getAllByText("Mécanique")).toHaveLength(2);
    expect(screen.getAllByRole("link", { name: "AB-123-CD" })[0]).toHaveAttribute("href", "/vehicles/v1");
  });

  it("conserve l'ordre renvoyé par l'API", async () => {
    renderView();
    await screen.findByText("Freins HS");
    const descriptions = screen
      .getAllByRole("row")
      .slice(1)
      .map((r) => within(r).getAllByRole("cell")[3].textContent);
    expect(descriptions).toEqual(["Freins HS", "Rétroviseur cassé", "Rayure"]);
  });

  it("transmet les filtres combinés à l'API", async () => {
    renderView();
    await screen.findByText("Freins HS");
    expect(api.listFleetProblems).toHaveBeenLastCalledWith({
      status: "",
      vehicleId: "",
      natureId: "",
    });
    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Statut" }));
    fireEvent.click(within(await screen.findByRole("listbox")).getByText("Résolu"));
    await waitFor(() =>
      expect(api.listFleetProblems).toHaveBeenLastCalledWith({
        status: "Resolved",
        vehicleId: "",
        natureId: "",
      }),
    );
    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Véhicule" }));
    fireEvent.click(within(await screen.findByRole("listbox")).getByText("AB-123-CD"));
    fireEvent.mouseDown(screen.getByRole("combobox", { name: "Nature" }));
    fireEvent.click(within(await screen.findByRole("listbox")).getByText("Mécanique"));
    await waitFor(() =>
      expect(api.listFleetProblems).toHaveBeenLastCalledWith({
        status: "Resolved",
        vehicleId: "v1",
        natureId: "n1",
      }),
    );
  });

  it("affiche un message sans problème", async () => {
    api.listFleetProblems.mockResolvedValue([]);
    renderView();
    expect(await screen.findByText("Aucun problème")).toBeInTheDocument();
  });
});
