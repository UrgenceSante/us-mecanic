import { Typography, type TypographyProps } from "@mui/material";
import { formatEuro } from "../format/formatEuro";

/** Montant en euros : aligné à droite, chiffres tabulaires pour que les colonnes se comparent d'un coup d'œil. */
export default function Amount({ value, sx, ...rest }: { value: number } & Omit<TypographyProps, "children">) {
  return (
    <Typography
      component="span"
      {...rest}
      sx={{ display: "block", textAlign: "right", fontVariantNumeric: "tabular-nums", whiteSpace: "nowrap", ...sx }}
    >
      {formatEuro(value)}
    </Typography>
  );
}
