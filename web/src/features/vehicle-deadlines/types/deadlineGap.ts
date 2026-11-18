import type { DeadlineStatus } from "./vehicleDeadline";

export interface DeadlineGapInput {
  /** Date ISO AAAA-MM-JJ. */
  dueDate: string | null;
  dueKm: number | null;
  /** Km actuel du véhicule, null sans relevé (le critère km est alors ignoré, comme côté API). */
  currentKm: number | null;
  status: DeadlineStatus | null;
}

export interface DeadlineGap {
  /** Texte lisible : « dépassée depuis 12 j », « dans 6 j », « dépassée de 1 200 km »… */
  text: string;
  /** Critère(s) qui rendent l'échéance dépassée (vide si elle ne l'est pas). */
  overdueBy: ("date" | "km")[];
}

const DAY_MS = 86_400_000;
const km = (n: number) => `${n.toLocaleString("fr-FR")} km`;

/** Date du jour (AAAA-MM-JJ) à l'heure de Paris, calendrier métier de l'API. */
export function todayInParis(now: Date = new Date()): string {
  return new Intl.DateTimeFormat("en-CA", { timeZone: "Europe/Paris" }).format(now);
}

const dayNumber = (iso: string) => Math.round(Date.parse(`${iso}T00:00:00Z`) / DAY_MS);

/**
 * Écart d'une échéance non close. Mêmes critères que l'API : dépassée si la date est strictement passée
 * ou si le km actuel atteint le km d'échéance ; le texte précise quel critère s'applique.
 * Renvoie null sans statut (échéance close ou statut inconnu) ou sans critère exploitable.
 */
export function deadlineGap({ dueDate, dueKm, currentKm, status }: DeadlineGapInput, today: string): DeadlineGap | null {
  if (status === null) return null;
  const days = dueDate ? dayNumber(dueDate) - dayNumber(today) : null;
  const kmLeft = dueKm !== null && currentKm !== null ? dueKm - currentKm : null;
  const overdueBy: DeadlineGap["overdueBy"] = [];
  const parts: string[] = [];

  if (days !== null && days < 0) {
    overdueBy.push("date");
    parts.push(`depuis ${-days} j`);
  }
  if (kmLeft !== null && kmLeft <= 0) {
    overdueBy.push("km");
    parts.push(`de ${km(-kmLeft)}`);
  }
  if (overdueBy.length > 0) return { text: `dépassée ${parts.join(" et ")}`, overdueBy };

  if (days !== null) parts.push(days === 0 ? "aujourd'hui" : `dans ${days} j`);
  if (kmLeft !== null) parts.push(`dans ${km(kmLeft)}`);
  return parts.length > 0 ? { text: parts.join(" ou "), overdueBy } : null;
}
