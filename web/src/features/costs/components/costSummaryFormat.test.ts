import { describe, expect, it } from "vitest";
import { currentYearRange, excludedLabel, familyTotal, formatPeriod, shortcutRange } from "./costSummaryFormat";

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

describe("shortcutRange", () => {
  // 1er janvier 00:30 en local : un calcul en UTC donnerait la veille.
  const now = new Date(2026, 0, 1, 0, 30);

  it("cette année", () => {
    expect(shortcutRange("thisYear", now)).toEqual({ from: "2026-01-01", to: "2026-12-31" });
  });

  it("mois dernier, en passant l'année", () => {
    expect(shortcutRange("lastMonth", now)).toEqual({ from: "2025-12-01", to: "2025-12-31" });
    expect(shortcutRange("lastMonth", new Date(2026, 2, 15))).toEqual({ from: "2026-02-01", to: "2026-02-28" });
  });

  it("12 derniers mois = mois courant compris", () => {
    expect(shortcutRange("last12Months", new Date(2026, 9, 31))).toEqual({ from: "2025-11-01", to: "2026-10-31" });
    expect(shortcutRange("last12Months", now)).toEqual({ from: "2025-02-01", to: "2026-01-31" });
  });
});

describe("excludedLabel", () => {
  it("accorde au singulier et au pluriel", () => {
    expect(excludedLabel(1)).toBe("1 véhicule exclu (moins de 2 relevés)");
    expect(excludedLabel(3)).toBe("3 véhicules exclus (moins de 2 relevés)");
  });
});
