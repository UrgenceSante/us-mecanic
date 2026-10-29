import { Alert, Button, Checkbox, FormControlLabel, Paper, Stack, TextField, Typography } from "@mui/material";
import { useState, type FormEvent } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { problemErrorMessage } from "../api/problemErrors";
import { useAnalyzeProblem } from "../api/problemMutations";
import ReferenceSelect from "./ReferenceSelect";

/** Saisie de l'analyse d'un problème « Signalé » : nature, élément, texte, immobilisation. */
export default function ProblemAnalysisForm({ problemId, vehicleId }: { problemId: string; vehicleId: string }) {
  const { notifySuccess } = useNotifSnack();
  const analyze = useAnalyzeProblem(problemId, vehicleId);
  const [natureId, setNatureId] = useState("");
  const [elementId, setElementId] = useState("");
  const [text, setText] = useState("");
  const [immobilized, setImmobilized] = useState(false);

  const invalid = natureId === "" || elementId === "" || text.trim() === "";

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (invalid) return;
    analyze.mutate(
      { natureId, elementId, analysis: text.trim(), immobilized },
      { onSuccess: () => notifySuccess("Problème analysé") },
    );
  };

  return (
    <Paper component="form" sx={{ padding: 2 }} onSubmit={handleSubmit}>
      <Typography variant="h6">Analyse</Typography>
      {analyze.isError && <Alert severity="error">{problemErrorMessage(analyze.error)}</Alert>}
      <Stack>
        <ReferenceSelect kind="nature" label="Nature" value={natureId} onChange={setNatureId} />
        <ReferenceSelect kind="element" label="Élément" value={elementId} onChange={setElementId} />
        <TextField
          label="Analyse"
          required
          fullWidth
          multiline
          minRows={3}
          margin="dense"
          value={text}
          onChange={(e) => setText(e.target.value)}
        />
        <FormControlLabel
          control={<Checkbox checked={immobilized} onChange={(e) => setImmobilized(e.target.checked)} />}
          label="Véhicule immobilisé"
        />
        <div>
          <Button type="submit" variant="contained" disabled={invalid || analyze.isPending}>
            Enregistrer l'analyse
          </Button>
        </div>
      </Stack>
    </Paper>
  );
}
