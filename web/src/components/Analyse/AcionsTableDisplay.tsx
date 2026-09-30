import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import type { IAction } from "./IAction";

interface ActionsTableDisplayProps {
  actions: IAction[];
}
export default function AcionsTableDisplay(props: ActionsTableDisplayProps) {
  if (props.actions.length < 1) return <Box>Aucune actions à entreprendre</Box>;

  return (
    <TableContainer>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Action</TableCell>
            <TableCell>Acteur</TableCell>
            <TableCell>Echéance</TableCell>
            <TableCell>Commentaire</TableCell>
          </TableRow>
        </TableHead>
        {/* prettier-ignore */}
        <TableBody>
          {props.actions.map((row) => (
            <TableRow>
              <TableCell>{row.ActionType?.Value || "Erreur Action type"}</TableCell>
              <TableCell>{row.Actor?.Value || "Erreur Actor"}</TableCell>
              <TableCell>{}</TableCell>
              <TableCell>{row.comment}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
