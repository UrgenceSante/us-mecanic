import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { IMecanicLog } from "../../../components/MecanicLog/IMecanicLog";
import { getMecanicLogs } from "../../../services/MecanicLog.service";
import { filterLogsByPlate } from "../api/vehicleProblemsQueries";
import VehicleProblems from "./VehicleProblems";

vi.mock("../../../services/MecanicLog.service", () => ({ getMecanicLogs: vi.fn() }));

const log = (id: number, plate: string | null, report: string) =>
  ({ LogId: id, Immatriculation: plate, Report: report, ReportState: "Ouvert", LogDate: "2025-01-02T10:00:00" }) as IMecanicLog;

const logs = [log(1, "AB-123-CD", "Frein HS"), log(2, "ZZ-999-ZZ", "Autre"), log(3, null, "Sans immat")];

function renderView(plate: string) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter>
        <VehicleProblems licensePlate={plate} />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("filterLogsByPlate", () => {
  it("ignore casse, tirets et espaces", () => {
    expect(filterLogsByPlate(logs, "ab 123 cd").map((l) => l.LogId)).toEqual([1]);
  });
});

describe("VehicleProblems", () => {
  beforeEach(() => vi.mocked(getMecanicLogs).mockReset());

  it("affiche uniquement les logs du véhicule avec un lien vers l'analyse", async () => {
    vi.mocked(getMecanicLogs).mockResolvedValue(logs);
    renderView("AB-123-CD");

    expect(await screen.findByText("Frein HS")).toBeInTheDocument();
    expect(screen.queryByText("Autre")).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Voir l'analyse" })).toHaveAttribute("href", "/analyse/1");
  });

  it("affiche un message explicite sans log", async () => {
    vi.mocked(getMecanicLogs).mockResolvedValue(logs);
    renderView("XX-000-XX");

    expect(await screen.findByText("Aucun problème enregistré pour ce véhicule.")).toBeInTheDocument();
  });
});
