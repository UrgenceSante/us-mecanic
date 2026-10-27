import { Alert, Button, Paper, Stack, TextField, Typography } from "@mui/material";
import { useState, type FormEvent } from "react";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { deadlineTypeErrorMessage } from "../api/deadlineTypeErrors";
import { useDeadlineThresholds, useUpdateDeadlineThresholds } from "../api/deadlineTypeQueries";
import type { DeadlineThresholds } from "../types/deadlineType";

function ThresholdsForm({ thresholds }: { thresholds: DeadlineThresholds }) {
  const { notifySuccess } = useNotifSnack();
  const update = useUpdateDeadlineThresholds();
  const [days, setDays] = useState(String(thresholds.dueSoonDays));
  const [km, setKm] = useState(String(thresholds.dueSoonKm));

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    update.mutate(
      { dueSoonDays: Number(days), dueSoonKm: Number(km) },
      { onSuccess: () => notifySuccess("Seuils globaux enregistrés") },
    );
  };

  return (
    <form onSubmit={handleSubmit}>
      {update.isError && (
        <Alert severity="error" sx={{ marginBottom: 2 }}>
          {deadlineTypeErrorMessage(update.error)}
        </Alert>
      )}
      <Stack direction={{ xs: "column", sm: "row" }} gap={2} alignItems="flex-start">
        <TextField
          label="Jours avant échéance"
          type="number"
          required
          value={days}
          slotProps={{ htmlInput: { min: 1 } }}
          onChange={(e) => setDays(e.target.value)}
        />
        <TextField
          label="Kilomètres avant échéance"
          type="number"
          required
          value={km}
          slotProps={{ htmlInput: { min: 1 } }}
          onChange={(e) => setKm(e.target.value)}
        />
        <Button type="submit" variant="contained" disabled={update.isPending} sx={{ alignSelf: "center" }}>
          Enregistrer les seuils
        </Button>
      </Stack>
    </form>
  );
}

/** Seuils globaux « échéance proche », appliqués aux types sans seuil propre. */
export default function DeadlineThresholdsSection() {
  const thresholds = useDeadlineThresholds();

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Typography variant="h6">Seuils globaux « échéance proche »</Typography>
        {thresholds.isError && <ErrorHandler error={thresholds.error} />}
        {thresholds.data && <ThresholdsForm thresholds={thresholds.data} />}
      </Stack>
    </Paper>
  );
}
