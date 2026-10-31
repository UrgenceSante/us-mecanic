import { describe, expect, it } from "vitest";
import { currentYearRange, familyTotal, formatPeriod } from "./costSummaryFormat";

describe("costSummaryFormat", () => {
  it("formate un mois en français", () => {
    expect(formatPeriod("2026-03", "month")).toBe("mars 2026");
  });
  it("formate une année", () => {
    expect(formatPeriod("2026", "year")).toBe("2026");
  });
  it("renvoie 0 pour une famille absente", () => {
    expect(familyTotal([{ family: "Loan", total: 5 }], "OneOff")).toBe(0);
  });
  it("calcule l'année en cours", () => {
    expect(currentYearRange(new Date(2026, 5, 1))).toEqual({ from: "2026-01-01", to: "2026-12-31" });
  });
});
