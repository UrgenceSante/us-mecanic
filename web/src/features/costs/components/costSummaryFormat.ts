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

function iso(d: Date): string {
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${m}-${day}`;
}

export type PeriodShortcut = "thisYear" | "last12Months" | "lastMonth";

export const periodShortcutLabels: Record<PeriodShortcut, string> = {
  thisYear: "Cette année",
  last12Months: "12 derniers mois",
  lastMonth: "Mois dernier",
};

/** Dates des raccourcis de période, calculées en date locale (jamais en UTC). */
export function shortcutRange(shortcut: PeriodShortcut, now = new Date()): { from: string; to: string } {
  const y = now.getFullYear();
  const m = now.getMonth();
  if (shortcut === "thisYear") return currentYearRange(now);
  if (shortcut === "lastMonth") return { from: iso(new Date(y, m - 1, 1)), to: iso(new Date(y, m, 0)) };
  return { from: iso(new Date(y, m - 11, 1)), to: iso(new Date(y, m + 1, 0)) };
}

/** Texte « N véhicule(s) exclu(s) (moins de 2 relevés) ». */
export function excludedLabel(count: number): string {
  return `${count} véhicule${count > 1 ? "s" : ""} exclu${count > 1 ? "s" : ""} (moins de 2 relevés)`;
}
