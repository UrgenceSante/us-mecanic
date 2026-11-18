import { Chip, Tooltip } from "@mui/material";
import type { ReactElement } from "react";

export type StatusTone = "success" | "warning" | "error" | "info" | "neutral";

// Le « info » MUI (#0288d1) n'atteint que 4,0:1 sur blanc : version assombrie (7,6:1).
const infoSx = { color: "#01579b", borderColor: "#01579b", "& .MuiChip-icon": { color: "#01579b" } };

export interface StatusChipProps {
  tone: StatusTone;
  icon: ReactElement;
  label: string;
  tooltip?: string;
  /** Nom du critère lu par les lecteurs d'écran : « Statut : Immobilisé ». Par défaut « Statut ». */
  category?: string;
  variant?: "filled" | "outlined";
  /** Remplace l'aria-label complet (« Catégorie : libellé » par défaut). */
  ariaLabel?: string;
}

/**
 * Puce de statut unique de l'application : icône + texte (l'information ne dépend pas de la couleur),
 * contraste d'au moins 4,5:1 (variante « outlined » sur fond blanc, texte coloré foncé) et aria-label complet.
 */
export default function StatusChip({ tone, icon, label, tooltip, category = "Statut", variant = "outlined", ariaLabel }: StatusChipProps) {
  const chip = (
    <Chip
      size="small"
      variant={variant}
      color={tone === "neutral" || tone === "info" ? "default" : tone}
      sx={tone === "info" ? infoSx : undefined}
      icon={icon}
      label={label}
      aria-label={ariaLabel ?? `${category} : ${label}`}
      role="img"
    />
  );
  return tooltip ? <Tooltip title={tooltip}>{chip}</Tooltip> : chip;
}
