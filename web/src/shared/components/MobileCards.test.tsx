import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { afterEach, describe, expect, it, vi } from "vitest";
import DashboardDeadlineTable from "../../features/dashboard/components/DashboardDeadlineTable";
import FleetProblemTable from "../../features/issues/components/FleetProblemTable";
import ProblemActionCards from "../../features/issues/components/ProblemActionCards";
import VehicleCards from "../../features/vehicles/components/VehicleCards";
import type { FleetProblem } from "../../features/issues/types/problem";
import type { Vehicle } from "../../features/vehicles/types/vehicle";

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

const deadlines = [
  {
    id: "d1",
    vehicle: { id: "v1", licensePlate: "AB-123-CD", internalNumber: "12", typeId: null, typeName: null },
    deadlineType: { id: "t1", name: "Contrôle technique" },
    status: "Overdue" as const,
    dueDate: "2026-03-04",
    dueKm: 90000,
    currentKm: 85000,
  },
];

const problems = [
  {
    problem: { id: "p1", vehicleId: "v1", date: "2026-03-04", description: "Freins HS", status: "Analyzed", actions: [] },
    vehicle: { id: "v1", licensePlate: "AB-123-CD" },
    nature: { id: "n1", name: "Mécanique" },
    blockingLevel: "Immobilizing",
  },
] as unknown as FleetProblem[];

const vehicles = [
  {
    id: "v1",
    licensePlate: "AB-123-CD",
    internalNumber: "12",
    typeName: "Ambulance",
    status: "Active",
    currentMileage: 85000,
  },
] as unknown as Vehicle[];

const action = {
  id: "a1",
  problemId: "p1",
  actorId: "x",
  typeId: "y",
  constraintId: "z",
  dueDate: "2026-04-01",
  comment: "Prévoir le garage",
  interventionId: null,
  isClosed: false,
  closedAt: null,
  recordedAt: "2026-03-04T08:00:00Z",
};

describe("repli en cartes sur mobile", () => {
  afterEach(() => vi.unstubAllGlobals());

  it("échéances : tableau sur grand écran, cartes sur mobile avec bouton", () => {
    mockMatchMedia(false);
    const onComplete = vi.fn();
    const { unmount } = render(
      <MemoryRouter>
        <DashboardDeadlineTable deadlines={deadlines} onComplete={onComplete} />
      </MemoryRouter>,
    );
    expect(screen.getByRole("table")).toBeInTheDocument();
    unmount();

    mockMatchMedia(true);
    render(
      <MemoryRouter>
        <DashboardDeadlineTable deadlines={deadlines} onComplete={onComplete} />
      </MemoryRouter>,
    );
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
    expect(screen.getByText("Contrôle technique", { exact: false })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: /Marquer réalisée/ }));
    expect(onComplete).toHaveBeenCalledWith(deadlines[0]);
  });

  it("problèmes du parc : cartes sur mobile", () => {
    mockMatchMedia(true);
    render(
      <MemoryRouter>
        <FleetProblemTable problems={problems} />
      </MemoryRouter>,
    );
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Freins HS" })).toHaveAttribute("href", "/problems/p1");
  });

  it("véhicules : carte cliquable", () => {
    render(
      <MemoryRouter>
        <VehicleCards vehicles={vehicles} />
      </MemoryRouter>,
    );
    expect(screen.getByRole("link", { name: "Véhicule AB-123-CD" })).toHaveAttribute("href", "/vehicles/v1");
    expect(screen.getByText(/Ambulance/)).toBeInTheDocument();
  });

  it("actions d'un problème : boutons Modifier / Clôturer", () => {
    const onEdit = vi.fn();
    const onClose = vi.fn();
    render(
      <ProblemActionCards
        actions={[action]}
        name={(kind) => kind}
        interventionLabel={() => "-"}
        onEdit={onEdit}
        onClose={onClose}
      />,
    );
    fireEvent.click(screen.getByRole("button", { name: "Modifier" }));
    fireEvent.click(screen.getByRole("button", { name: "Clôturer" }));
    expect(onEdit).toHaveBeenCalledWith(action);
    expect(onClose).toHaveBeenCalledWith(action);
  });
});
