import { Box, CardActionArea, Paper } from "@mui/material";
import type { ReactNode } from "react";
import { Link } from "react-router";

interface ListCardProps {
  children: ReactNode;
  /** Si renseigné, toute la carte est un lien. */
  to?: string;
  "aria-label"?: string;
}

/** Carte de repli mobile d'une ligne de tableau. */
export default function ListCard({ children, to, "aria-label": ariaLabel }: ListCardProps) {
  return (
    <Paper variant="outlined" data-testid="list-card">
      {to ? (
        <CardActionArea component={Link} to={to} aria-label={ariaLabel} sx={{ padding: 1.5 }}>
          {children}
        </CardActionArea>
      ) : (
        <Box sx={{ padding: 1.5 }}>{children}</Box>
      )}
    </Paper>
  );
}
