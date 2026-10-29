import type { BlockingLevel, ProblemStatus } from "./problem";

export const problemStatusLabels: Record<ProblemStatus, string> = {
  Reported: "Signalé",
  Analyzed: "Analysé",
  Resolved: "Résolu",
};

export const blockingLevelLabels: Record<BlockingLevel, string> = {
  Immobilizing: "Immobilisant",
  Unanalyzed: "Non analysé",
  OverdueAction: "Action en retard",
  Other: "Autre",
};

export const blockingLevelColors: Record<BlockingLevel, "error" | "warning" | "info" | "default"> = {
  Immobilizing: "error",
  Unanalyzed: "warning",
  OverdueAction: "info",
  Other: "default",
};
