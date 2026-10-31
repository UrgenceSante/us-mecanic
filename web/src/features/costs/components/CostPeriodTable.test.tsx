import { render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import CostPeriodTable from "./CostPeriodTable";

const periods = [
  {
    period: "2026-03",
    start: "2026-03-01",
    total: 150,
    families: [
      { family: "OneOff" as const, total: 100 },
      { family: "Running" as const, total: 50 },
      { family: "Loan" as const, total: 0 },
    ],
    categories: [],
  },
];

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

describe("CostPeriodTable", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("affiche un tableau sur grand écran", () => {
    mockMatchMedia(false);
    render(<CostPeriodTable periods={periods} granularity="month" />);
    expect(screen.getByRole("table")).toBeInTheDocument();
  });

  it("empile les lignes sur mobile (pas de tableau)", () => {
    mockMatchMedia(true);
    render(<CostPeriodTable periods={periods} granularity="month" />);
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
    expect(screen.getByRole("listitem")).toHaveTextContent("mars 2026");
    expect(screen.getByRole("listitem")).toHaveTextContent("Ponctuelles");
  });
});
