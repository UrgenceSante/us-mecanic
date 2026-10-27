/** Date locale du jour au format AAAA-MM-JJ (pas d'UTC : évite le décalage en fin de journée). */
export function todayLocalIso(now = new Date()): string {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}
