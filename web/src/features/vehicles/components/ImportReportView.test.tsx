import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { VehicleImportReport } from "../types/vehicle";
import ImportReportView from "./ImportReportView";

const report: VehicleImportReport = {
  dryRun: true,
  created: 1,
  updated: 0,
  unchanged: 0,
  errors: 1,
  lines: [
    { line: 2, licensePlate: "AB-123-CD", outcome: "Created", errorCode: null, message: null, owner: "Ambulances Nord" },
    {
      line: 3,
      licensePlate: "EF-456-GH",
      outcome: "Error",
      errorCode: "import.unknown_company",
      message: "Société inconnue ou inactive : Nord",
      owner: null,
    },
  ],
};

describe("ImportReportView", () => {
  it("affiche le propriétaire appliqué dans le bilan", () => {
    render(<ImportReportView report={report} />);
    expect(screen.getByRole("columnheader", { name: "Propriétaire" })).toBeInTheDocument();
    expect(screen.getByText("Ambulances Nord")).toBeInTheDocument();
    expect(screen.getByText("Société inconnue ou inactive : Nord")).toBeInTheDocument();
  });
});
