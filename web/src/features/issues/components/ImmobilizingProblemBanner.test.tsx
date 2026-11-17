import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import ImmobilizingProblemBanner from "./ImmobilizingProblemBanner";

const api = vi.hoisted(() => ({ listVehicleProblems: vi.fn() }));
vi.mock("../api/problemsApi", () => api);

const analysis = (immobilized: boolean) => ({
  natureId: "n1",
  elementId: "e1",
  text: "",
  immobilized,
  analyzedAt: "2026-03-05T08:00:00Z",
});
const problem = (id: string, description: string, status: string, immobilized: boolean | null) => ({
  id,
  vehicleId: "v1",
  date: "2026-03-04",
  description,
  author: "Léa",
  status,
  recordedAt: "2026-03-04T08:00:00Z",
  analysis: immobilized === null ? null : analysis(immobilized),
  actions: [],
});

function renderBanner() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <ImmobilizingProblemBanner vehicleId="v1" />
      </MemoryRouter>
    </QueryClientProvider>,
  );
  return client;
}

describe("ImmobilizingProblemBanner", () => {
  beforeEach(() => vi.resetAllMocks());

  it("affiche le bandeau avec un lien vers le problème immobilisant ouvert", async () => {
    api.listVehicleProblems.mockResolvedValue([
      problem("p1", "Bruit au freinage", "Analyzed", false),
      problem("p2", "Fuite d'huile", "Analyzed", true),
    ]);
    renderBanner();
    const link = await screen.findByRole("link", { name: "Fuite d'huile" });
    expect(link).toHaveAttribute("href", "/problems/p2");
    expect(screen.getByRole("alert")).toHaveTextContent("Problème immobilisant en cours : Fuite d'huile");
  });

  it("n'affiche rien si le problème immobilisant est résolu ou s'il n'y en a pas", async () => {
    api.listVehicleProblems.mockResolvedValue([
      problem("p1", "Fuite d'huile", "Resolved", true),
      problem("p3", "Vitre", "Reported", null),
    ]);
    const client = renderBanner();
    await vi.waitFor(() => expect(client.getQueryState(["problems", "vehicle", "v1"])?.status).toBe("success"));
    expect(screen.queryByRole("alert")).toBeNull();
  });
});
