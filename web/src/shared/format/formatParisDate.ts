/** Date (JJ/MM/AAAA) d'un horodatage ISO, à l'heure de Paris : évite le décalage d'un jour autour de minuit UTC. */
const parisDate = new Intl.DateTimeFormat("fr-FR", { timeZone: "Europe/Paris", day: "2-digit", month: "2-digit", year: "numeric" });

export function formatParisDate(timestamp: string): string {
  return parisDate.format(new Date(timestamp));
}
