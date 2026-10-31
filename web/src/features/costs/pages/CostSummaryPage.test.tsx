import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import CostSummaryPage from "./CostSummaryPage";

const api = vi.hoisted(() => ({ getCostSummary: vi.fn() }));
vi.mock("../api/costSummaryApi", () => api);
vi.mock("../../vehicles/api/vehicleQueries", () => ({
  useVehicles: () => ({ data: [{ id: "v1", licensePlate: "AB-123-CD" }] }),
}));

function renderPage(url = "/costs") {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={[url]}>
        <CostSummaryPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

const families = (a: number, b: number, c: number) => [
  { family: "OneOff", total: a },
  { family: "Running", total: b },
  { family: "Loan", total: c },
];

function summary(over: object = {}) {
  return {
    from: "2026-01-01",
    to: "2026-12-31",
    vehicleId: null,
    total: 150,
    families: families(100, 50, 0),
    categories: [{ family: "OneOff", categoryId: "c1", category: "Pneus", total: 100 }],
    periods: [{ period: "2026-03", start: "2026-03-01", total: 150, families: families(100, 50, 0), categories: [] }],
    costPerKm: { kilometers: 0, cost: 0, value: null, computable: false, excludedVehicles: 2 },
    ...over,
  };
}

describe("CostSummaryPage", () => {
  beforeEach(() => vi.resetAllMocks());

  it("affiche cartes, période en français, catégories et coût au km non calculable", async () => {
    api.getCostSummary.mockResolvedValue(summary());
    renderPage();

    expect(screen.getByLabelText("Chargement")).toBeInTheDocument();
    expect(await screen.findByText("mars 2026")).toBeInTheDocument();
    expect(screen.getByText("Non calculable")).toBeInTheDocument();
    expect(screen.getByText(/2 véhicules exclus/)).toBeInTheDocument();
    expect(screen.getByText("Pneus")).toBeInTheDocument();
    expect(screen.getAllByText("Emprunts").length).toBeGreaterThan(0);
  });

  it("transmet le véhicule de l'URL et affiche le coût au km", async () => {
    api.getCostSummary.mockResolvedValue(
      summary({ vehicleId: "v1", costPerKm: { kilometers: 1000, cost: 150, value: 0.15, computable: true, excludedVehicles: 0 } }),
    );
    renderPage("/costs?vehicleId=v1");

    expect(await screen.findByText(/0,15\s€ \/ km/)).toBeInTheDocument();
    expect(api.getCostSummary).toHaveBeenCalledWith(expect.objectContaining({ vehicleId: "v1", granularity: "month" }));
  });

  it("affiche km parcourus, exclusions et applique un raccourci de période", async () => {
    api.getCostSummary.mockResolvedValue(
      summary({ costPerKm: { kilometers: 12000, cost: 1800, value: 0.15, computable: true, excludedVehicles: 3 } }),
    );
    renderPage();

    expect(await screen.findByText(/12\s000 km parcourus/)).toBeInTheDocument();
    expect(screen.getByText("3 véhicules exclus (moins de 2 relevés)")).toBeInTheDocument();
    expect(screen.getByLabelText("Sous-total Ponctuelles")).toHaveTextContent("100,00");

    fireEvent.click(screen.getByRole("button", { name: "Mois dernier" }));
    await waitFor(() =>
      expect(api.getCostSummary).toHaveBeenLastCalledWith(expect.objectContaining({ to: expect.stringMatching(/^\d{4}-\d{2}-(28|29|30|31)$/) })),
    );
  });

  it("affiche l'état vide", async () => {
    api.getCostSummary.mockResolvedValue(summary({ total: 0, periods: [], categories: [] }));
    renderPage();
    expect(await screen.findByText("Aucune dépense sur cette période.")).toBeInTheDocument();
  });

  it("affiche l'erreur", async () => {
    api.getCostSummary.mockRejectedValue({
      isAxiosError: true,
      response: { status: 400, data: { title: "x", code: "cost_summary.invalid_range" } },
    });
    renderPage();
    expect(await screen.findByText(/doit précéder/)).toBeInTheDocument();
  });
});
