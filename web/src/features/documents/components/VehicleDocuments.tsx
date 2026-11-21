import AddIcon from "@mui/icons-material/Add";
import { Alert, Button, CircularProgress, MenuItem, Paper, Stack, TextField, Typography } from "@mui/material";
import { useMemo, useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { useDocumentCategories } from "../../document-categories/api/documentCategoryQueries";
import { documentErrorMessage } from "../api/documentErrors";
import { useAddDocument, useDeleteDocument, useDocuments } from "../api/documentQueries";
import type { VehicleDocument } from "../types/document";
import AddDocumentDialog from "./AddDocumentDialog";
import DeleteDocumentDialog from "./DeleteDocumentDialog";
import DocumentList from "./DocumentList";
import { downloadDocument, openDocument } from "./documentBlob";

interface VehicleDocumentsProps {
  vehicleId: string;
}

/** Section « Documents » de la fiche véhicule. Un véhicule sorti du parc peut recevoir des documents. */
export default function VehicleDocuments({ vehicleId }: VehicleDocumentsProps) {
  const { notifySuccess, notifyError } = useNotifSnack();
  const [categoryId, setCategoryId] = useState<string | null>(null);
  const [adding, setAdding] = useState(false);
  const [deleting, setDeleting] = useState<VehicleDocument | null>(null);
  const documents = useDocuments(vehicleId, categoryId);
  const categories = useDocumentCategories(true);
  const add = useAddDocument(vehicleId);
  const remove = useDeleteDocument(vehicleId);

  const categoryNames = useMemo(() => new Map((categories.data ?? []).map((c) => [c.id, c.name])), [categories.data]);

  const closeAdd = () => {
    setAdding(false);
    add.reset();
  };
  const closeDelete = () => {
    setDeleting(null);
    remove.reset();
  };

  // Pas de async/await ici : l'ouverture de l'onglet doit rester synchrone dans le clic (iOS).
  const run = (action: Promise<void>) => action.catch((error: unknown) => notifyError(documentErrorMessage(error)));
  const open = (document: VehicleDocument) => void run(openDocument(document));
  const download = (document: VehicleDocument) => void run(downloadDocument(document));

  const confirmDelete = () =>
    deleting &&
    remove.mutate(deleting.id, {
      onSuccess: () => {
        notifySuccess("Document supprimé");
        closeDelete();
      },
    });

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
          <Typography variant="h6" component="h2">Documents</Typography>
          <Stack flexGrow={1} />
          <TextField
            select
            size="small"
            label="Catégorie"
            value={categoryId ?? ""}
            onChange={(event) => setCategoryId(event.target.value || null)}
            sx={{ minWidth: 200 }}
          >
            <MenuItem value="">Toutes</MenuItem>
            {(categories.data ?? []).map((category) => (
              <MenuItem key={category.id} value={category.id}>
                {category.name}
              </MenuItem>
            ))}
          </TextField>
          <Button startIcon={<AddIcon />} onClick={() => setAdding(true)}>
            Ajouter un document
          </Button>
        </Stack>
        {documents.isLoading && <CircularProgress size={24} aria-label="Chargement des documents" />}
        {documents.isError && <Alert severity="error">{documentErrorMessage(documents.error)}</Alert>}
        {documents.data?.length === 0 && <Typography color="text.secondary">Aucun document</Typography>}
        {documents.data && documents.data.length > 0 && (
          <DocumentList documents={documents.data} categoryNames={categoryNames} onOpen={open} onDownload={download} onDelete={setDeleting} />
        )}
      </Stack>
      {adding && (
        <AddDocumentDialog
          submitting={add.isPending}
          error={add.isError ? documentErrorMessage(add.error) : null}
          onSubmit={(upload) =>
            add.mutate(upload, {
              onSuccess: () => {
                notifySuccess("Document ajouté");
                closeAdd();
              },
            })
          }
          onClose={closeAdd}
        />
      )}
      {deleting && (
        <DeleteDocumentDialog
          document={deleting}
          submitting={remove.isPending}
          error={remove.isError ? documentErrorMessage(remove.error) : null}
          onConfirm={confirmDelete}
          onClose={closeDelete}
        />
      )}
    </Paper>
  );
}
