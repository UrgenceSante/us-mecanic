import type { DeadlineType } from "./deadlineType";

const nf = new Intl.NumberFormat("fr-FR");

/** « 12 mois / 30 000 km », une seule moitié si l'autre est absente, « Ponctuel » sans périodicité. */
export function formatPeriodicity(type: DeadlineType): string {
  if (type.isOneOff) return "Ponctuel";
  const parts: string[] = [];
  if (type.defaultIntervalMonths !== null) parts.push(`${type.defaultIntervalMonths} mois`);
  if (type.defaultIntervalKm !== null) parts.push(`${nf.format(type.defaultIntervalKm)} km`);
  return parts.join(" / ");
}

/** Seuils propres du type, ou « seuils globaux » quand aucun n'est défini. */
export function formatThresholds(type: DeadlineType): string {
  const parts: string[] = [];
  if (type.dueSoonDays !== null) parts.push(`${type.dueSoonDays} j`);
  if (type.dueSoonKm !== null) parts.push(`${nf.format(type.dueSoonKm)} km`);
  return parts.length > 0 ? parts.join(" / ") : "seuils globaux";
}
