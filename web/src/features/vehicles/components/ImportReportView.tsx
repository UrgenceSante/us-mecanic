import AddCircleIcon from "@mui/icons-material/AddCircle";
import ErrorIcon from "@mui/icons-material/Error";
import PublishedWithChangesIcon from "@mui/icons-material/PublishedWithChanges";
import RemoveCircleOutlineIcon from "@mui/icons-material/RemoveCircleOutline";
import type { ReactElement } from "react";
import StatusChip, { type StatusTone } from "../../../shared/components/StatusChip";
import {
  Alert,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
} from "@mui/material";
import type { ImportOutcome, VehicleImportReport } from "../types/vehicle";

const outcomeLabels: Record<ImportOutcome, string> = {
  Created: "Créé",
  Updated: "Mis à jour",
  Unchanged: "Inchangé",
  Error: "Erreur",
};

const outcomeChips: Record<ImportOutcome, { tone: StatusTone; icon: ReactElement }> = {
  Created: { tone: "success", icon: <AddCircleIcon /> },
  Updated: { tone: "info", icon: <PublishedWithChangesIcon /> },
  Unchanged: { tone: "neutral", icon: <RemoveCircleOutlineIcon /> },
  Error: { tone: "error", icon: <ErrorIcon /> },
};

export default function ImportReportView({ report }: { report: VehicleImportReport }) {
  const lines = [...report.lines].sort(
    (a, b) => Number(b.outcome === "Error") - Number(a.outcome === "Error") || a.line - b.line,
  );

  return (
    <Stack gap={2}>
      <Alert severity={report.errors > 0 ? "warning" : "success"}>
        {report.dryRun ? "Simulation : rien n'a été enregistré. " : "Import terminé. "}
        {report.created} créé(s), {report.updated} mis à jour, {report.unchanged} inchangé(s),{" "}
        {report.errors} en erreur{report.dryRun && report.errors > 0 ? " (ces lignes seront ignorées)" : ""}.
      </Alert>
      <TableContainer component={Paper} sx={{ maxHeight: 420 }}>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>Ligne</TableCell>
              <TableCell>Immatriculation</TableCell>
              <TableCell>Résultat</TableCell>
              <TableCell>Propriétaire</TableCell>
              <TableCell>Détail</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {lines.map((line) => (
              <TableRow key={line.line}>
                <TableCell>{line.line}</TableCell>
                <TableCell>{line.licensePlate ?? "-"}</TableCell>
                <TableCell>
                  <StatusChip {...outcomeChips[line.outcome]} category="Résultat" label={outcomeLabels[line.outcome]} />
                </TableCell>
                <TableCell>{line.owner ?? "-"}</TableCell>
                <TableCell>{line.message ?? ""}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Stack>
  );
}
