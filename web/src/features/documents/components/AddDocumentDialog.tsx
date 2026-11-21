import { Alert, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField } from "@mui/material";
import { useState } from "react";
import { useDocumentCategories } from "../../document-categories/api/documentCategoryQueries";
import type { DocumentUpload } from "../types/document";
import DocumentFilePicker from "./DocumentFilePicker";

interface AddDocumentDialogProps {
  submitting: boolean;
  /** Erreur de l'API : la saisie est conservée. */
  error: string | null;
  onSubmit: (upload: DocumentUpload) => void;
  onClose: () => void;
}

export default function AddDocumentDialog({ submitting, error, onSubmit, onClose }: AddDocumentDialogProps) {
  const categories = useDocumentCategories(false);
  const [file, setFile] = useState<File | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [categoryId, setCategoryId] = useState("");
  const [label, setLabel] = useState("");
  const [submitted, setSubmitted] = useState(false);

  const missingFile = submitted && !file && !fileError;
  const missingCategory = submitted && categoryId === "";

  const submit = () => {
    setSubmitted(true);
    if (!file || categoryId === "") return;
    onSubmit({ file, categoryId, label });
  };

  return (
    <Dialog open onClose={submitting ? undefined : onClose} fullWidth maxWidth="sm">
      <DialogTitle>Ajouter un document</DialogTitle>
      <DialogContent>
        <Stack gap={2} paddingTop={1}>
          {error && <Alert severity="error">{error}</Alert>}
          <DocumentFilePicker
            file={file}
            error={fileError ?? (missingFile ? "Le fichier est obligatoire." : null)}
            onChange={(picked, problem) => {
              setFile(picked);
              setFileError(problem);
            }}
          />
          <TextField
            select
            required
            label="Catégorie"
            value={categoryId}
            onChange={(event) => setCategoryId(event.target.value)}
            error={missingCategory}
            helperText={missingCategory ? "La catégorie est obligatoire." : undefined}
          >
            {(categories.data ?? []).map((category) => (
              <MenuItem key={category.id} value={category.id}>
                {category.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Libellé (facultatif)"
            value={label}
            onChange={(event) => setLabel(event.target.value)}
            slotProps={{ htmlInput: { maxLength: 200 } }}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={submitting}>
          Annuler
        </Button>
        <Button
          variant="contained"
          disabled={submitting}
          onClick={submit}
          startIcon={submitting ? <CircularProgress size={16} color="inherit" aria-hidden /> : undefined}
        >
          {submitting ? "Envoi en cours…" : "Ajouter"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
