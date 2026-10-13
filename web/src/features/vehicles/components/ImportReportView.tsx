import {
  Alert,
  Chip,
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

const outcomeColors = {
  Created: "success",
  Updated: "info",
  Unchanged: "default",
  Error: "error",
} as const;

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
              <TableCell>Détail</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {lines.map((line) => (
              <TableRow key={line.line}>
                <TableCell>{line.line}</TableCell>
                <TableCell>{line.licensePlate ?? "-"}</TableCell>
                <TableCell>
                  <Chip size="small" color={outcomeColors[line.outcome]} label={outcomeLabels[line.outcome]} />
                </TableCell>
                <TableCell>{line.message ?? ""}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Stack>
  );
}
