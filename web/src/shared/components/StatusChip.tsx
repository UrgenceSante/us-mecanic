import { Chip, Tooltip } from "@mui/material";
import type { ReactElement } from "react";

export type StatusTone = "success" | "warning" | "error" | "info" | "neutral";

export interface StatusChipProps {
  tone: StatusTone;
  icon: ReactElement;
  label: string;
  tooltip?: string;
  /** Nom du critère lu par les lecteurs d'écran : « Statut : Immobilisé ». Par défaut « Statut ». */
  category?: string;
  variant?: "filled" | "outlined";
}

/**
 * Puce de statut unique de l'application : icône + texte (l'information ne dépend pas de la couleur),
 * contraste d'au moins 4,5:1 (variante « outlined » sur fond blanc, texte coloré foncé) et aria-label complet.
 */
export default function StatusChip({ tone, icon, label, tooltip, category = "Statut", variant = "outlined" }: StatusChipProps) {
  const chip = (
    <Chip
      size="small"
      variant={variant}
      color={tone === "neutral" ? "default" : tone}
      icon={icon}
      label={label}
      aria-label={`${category} : ${label}`}
      role="img"
    />
  );
  return tooltip ? <Tooltip title={tooltip}>{chip}</Tooltip> : chip;
}
