import { TableCell } from "@mui/material";

const visuallyHidden = {
  border: 0,
  clip: "rect(0 0 0 0)",
  height: "1px",
  margin: "-1px",
  overflow: "hidden",
  padding: 0,
  position: "absolute",
  whiteSpace: "nowrap",
  width: "1px",
} as const;

/** En-tête de colonne d'actions : libellé « Actions » masqué visuellement mais lu par les lecteurs d'écran. */
export default function ActionsHeaderCell() {
  return (
    <TableCell>
      <span style={visuallyHidden}>Actions</span>
    </TableCell>
  );
}
