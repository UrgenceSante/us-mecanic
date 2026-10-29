import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import VehicleProblems from "./VehicleProblems";

const api = vi.hoisted(() => ({ listVehicleProblems: vi.fn() }));
const referencesApi = vi.hoisted(() => ({ listProblemReferences: vi.fn() }));
vi.mock("../api/problemsApi", () => api);
vi.mock("../../problem-references/api/problemReferencesApi", () => referencesApi);

const base = {
  vehicleId: "v1",
  recordedAt: "2026-03-04T08:00:00Z",
  actions: [],
};

function renderView() {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  render(
    <QueryClientProvider client={client}>
      <VehicleProblems vehicleId="v1" />
    </QueryClientProvider>,
  );
}

describe("VehicleProblems", () => {
  beforeEach(() => {
    vi.resetAllMocks();
    referencesApi.listProblemReferences.mockResolvedValue([{ id: "n1", name: "Mécanique", isActive: true }]);
  });

  it("affiche les problèmes du véhicule dans l'ordre de l'API avec l'analyse résumée", async () => {
    api.listVehicleProblems.mockResolvedValue([
      {
        ...base,
        id: "p2",
        date: "2026-03-04",
        description: "Freins HS",
        author: "Jean",
        status: "Analyzed",
        analysis: {
          natureId: "n1",
          elementId: "e1",
          text: "Plaquettes",
          immobilized: true,
          analyzedAt: "2026-03-05T08:00:00Z",
        },
      },
      {
        ...base,
        id: "p1",
        date: "2026-01-02",
        description: "Phare",
        author: "Marie",
        status: "Reported",
        analysis: null,
      },
    ]);
    renderView();

    expect(await screen.findByText("Freins HS")).toBeInTheDocument();
    expect(api.listVehicleProblems).toHaveBeenCalledWith("v1");
    const items = screen.getAllByRole("listitem");
    expect(items.map((i) => within(i).getByText(/Freins HS|Phare/).textContent)).toEqual(["Freins HS", "Phare"]);
    expect(within(items[0]).getByText("04/03/2026")).toBeInTheDocument();
    expect(within(items[0]).getByText("Analysé")).toBeInTheDocument();
    expect(within(items[0]).getByText("Jean")).toBeInTheDocument();
    expect(await within(items[0]).findByText("Mécanique · Immobilisant · Plaquettes")).toBeInTheDocument();
    expect(within(items[1]).getByText("Signalé")).toBeInTheDocument();
  });

  it("affiche un message sans problème", async () => {
    api.listVehicleProblems.mockResolvedValue([]);
    renderView();
    expect(await screen.findByText("Aucun problème enregistré pour ce véhicule.")).toBeInTheDocument();
  });
});
