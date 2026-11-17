import type { DashboardDeadline } from "../types/dashboard";

const rank = (d: DashboardDeadline) => (d.status === "Overdue" ? 0 : 1);

/** Dépassées avant proches, puis date d'échéance croissante (sans date en dernier). Ne modifie pas l'entrée. */
export function sortDeadlines(deadlines: DashboardDeadline[]): DashboardDeadline[] {
  return [...deadlines].sort((a, b) => {
    if (rank(a) !== rank(b)) return rank(a) - rank(b);
    if (a.dueDate === b.dueDate) return 0;
    if (a.dueDate === null) return 1;
    if (b.dueDate === null) return -1;
    return a.dueDate.localeCompare(b.dueDate);
  });
}

export function countDeadlines(deadlines: DashboardDeadline[]): { overdue: number; dueSoon: number } {
  const overdue = deadlines.filter((d) => d.status === "Overdue").length;
  return { overdue, dueSoon: deadlines.length - overdue };
}
