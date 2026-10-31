import type { CostFamily, CostFamilyTotal, CostGranularity } from "../types/costSummary";

export const costFamilyLabels: Record<CostFamily, string> = {
  OneOff: "Ponctuelles",
  Running: "Utilisation courante",
  Loan: "Emprunts",
};

export const costFamilies: CostFamily[] = ["OneOff", "Running", "Loan"];

const monthFormat = new Intl.DateTimeFormat("fr-FR", { month: "long", year: "numeric" });

/** « 2026-03 » -> « mars 2026 » ; « 2026 » reste « 2026 ». */
export function formatPeriod(period: string, granularity: CostGranularity): string {
  if (granularity === "year") return period.slice(0, 4);
  const [year, month] = period.split("-").map(Number);
  return monthFormat.format(new Date(year, month - 1, 1));
}

export function familyTotal(families: CostFamilyTotal[], family: CostFamily): number {
  return families.find((f) => f.family === family)?.total ?? 0;
}

export function currentYearRange(now = new Date()): { from: string; to: string } {
  const year = now.getFullYear();
  return { from: `${year}-01-01`, to: `${year}-12-31` };
}
