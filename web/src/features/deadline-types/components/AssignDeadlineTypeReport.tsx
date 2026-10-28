import { Alert, List, ListItem, ListItemText, Stack } from "@mui/material";
import type { AssignDeadlineTypeResult } from "../types/deadlineType";

/** Bilan d'un ajout en lot : ajoutés, ignorés (déjà en cours, sortis du parc) et véhicules en erreur. */
export default function AssignDeadlineTypeReport({ result }: { result: AssignDeadlineTypeResult }) {
  return (
    <Stack gap={1}>
      <Alert severity={result.added > 0 ? "success" : "info"}>{result.added} échéance(s) ajoutée(s).</Alert>
      <Alert severity="info">
        Ignorés : {result.skippedAlreadyActive} déjà concerné(s), {result.skippedRetired} sorti(s) du parc.
      </Alert>
      {result.errors.length > 0 && (
        <Alert severity="warning">
          {result.errors.length} véhicule(s) en erreur :
          <List dense disablePadding>
            {result.errors.map((error) => (
              <ListItem key={error.vehicleId} disableGutters>
                <ListItemText primary={error.licensePlate} secondary={error.message} />
              </ListItem>
            ))}
          </List>
        </Alert>
      )}
    </Stack>
  );
}
