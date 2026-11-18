import BlockIcon from "@mui/icons-material/Block";
import ErrorIcon from "@mui/icons-material/Error";
import StatusChip from "./StatusChip";

export interface UrgencyAlerts {
  overdueDeadlines: number;
  immobilizingProblem: boolean;
}

const plural = (n: number) => (n > 1 ? "s" : "");

/**
 * Alertes d'un véhicule (échéances dépassées, problème immobilisant) en une seule puce compacte,
 * pour tenir dans une ligne de tableau à hauteur fixe : « 2 éch. · immobilisant ».
 */
export default function UrgencyBadge({ alerts }: { alerts: UrgencyAlerts | null | undefined }) {
  if (!alerts || (!alerts.immobilizingProblem && alerts.overdueDeadlines === 0)) return null;
  const short: string[] = [];
  const long: string[] = [];
  if (alerts.overdueDeadlines > 0) {
    short.push(`${alerts.overdueDeadlines} éch.`);
    long.push(`${alerts.overdueDeadlines} échéance${plural(alerts.overdueDeadlines)} dépassée${plural(alerts.overdueDeadlines)}`);
  }
  if (alerts.immobilizingProblem) {
    short.push("immobilisant");
    long.push("problème immobilisant");
  }
  return (
    <StatusChip
      tone="error"
      icon={alerts.immobilizingProblem ? <BlockIcon /> : <ErrorIcon />}
      label={short.join(" · ")}
      ariaLabel={`Alerte : ${long.join(", ")}`}
      tooltip={long.join(", ")}
    />
  );
}
