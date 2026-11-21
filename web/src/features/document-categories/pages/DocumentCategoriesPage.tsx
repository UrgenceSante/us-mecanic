import {
  Alert,
  CircularProgress,
  Button,
  IconButton,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import BlockIcon from "@mui/icons-material/Block";
import EditIcon from "@mui/icons-material/Edit";
import { useState } from "react";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import ActiveStatusChip from "../../../shared/components/ActiveStatusChip";
import PageTitle from "../../../shared/components/PageTitle";
import useBelow from "../../../shared/hooks/useBelow";
import useNotifSnack from "../../../hooks/useNotifSnack";
import {
  useCreateDocumentCategory,
  useDeactivateDocumentCategory,
  useDocumentCategories,
  useRenameDocumentCategory,
} from "../api/documentCategoryQueries";
import { documentCategoryErrorMessage } from "../api/documentCategoryErrors";
import DocumentCategoryCards from "../components/DocumentCategoryCards";
import DocumentCategoryNameDialog from "../components/DocumentCategoryNameDialog";
import type { DocumentCategory } from "../types/documentCategory";

/** `null` : création ; une catégorie : renommage ; `undefined` : dialogue fermé. */
type Editing = DocumentCategory | null | undefined;

export default function DocumentCategoriesPage() {
  const { notifySuccess, notifyError } = useNotifSnack();
  const compact = useBelow("sm");
  const [editing, setEditing] = useState<Editing>(undefined);

  const categories = useDocumentCategories(true);
  const create = useCreateDocumentCategory();
  const rename = useRenameDocumentCategory();
  const deactivate = useDeactivateDocumentCategory();

  const mutation = editing ? rename : create;

  const closeDialog = () => {
    setEditing(undefined);
    create.reset();
    rename.reset();
  };

  const handleSubmit = (name: string) => {
    const onSuccess = () => {
      notifySuccess(editing ? "Catégorie renommée" : "Catégorie ajoutée");
      closeDialog();
    };
    if (editing) rename.mutate({ id: editing.id, name }, { onSuccess });
    else create.mutate(name, { onSuccess });
  };

  const handleDeactivate = (category: DocumentCategory) =>
    deactivate.mutate(category.id, {
      onSuccess: () => notifySuccess(`Catégorie « ${category.name} » désactivée`),
      onError: (error) => notifyError(documentCategoryErrorMessage(error)),
    });

  return (
    <Stack gap={2} marginY={2}>
      <Stack direction="row" alignItems="center" gap={1}>
        <PageTitle flexGrow={1}>Catégories de document</PageTitle>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing(null)}>
          Ajouter
        </Button>
      </Stack>
      <Alert severity="info">
        Une catégorie désactivée n'est plus proposée à l'ajout d'un document, mais reste affichée sur les documents déjà classés.
      </Alert>

      {categories.isError ? (
        <ErrorHandler error={categories.error} />
      ) : categories.isLoading ? (
        <CircularProgress aria-label="Chargement" sx={{ alignSelf: "center" }} />
      ) : compact ? (
        <Paper>
          <DocumentCategoryCards
            categories={categories.data ?? []}
            deactivating={deactivate.isPending}
            onRename={setEditing}
            onDeactivate={handleDeactivate}
          />
        </Paper>
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Nom</TableCell>
                <TableCell>Statut</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {(categories.data ?? []).map((category) => (
                <TableRow key={category.id}>
                  <TableCell>{category.name}</TableCell>
                  <TableCell>
                    <ActiveStatusChip isActive={category.isActive} feminine />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Renommer">
                      <IconButton aria-label={`Renommer ${category.name}`} onClick={() => setEditing(category)}>
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    {category.isActive && (
                      <Tooltip title="Désactiver">
                        <IconButton
                          aria-label={`Désactiver ${category.name}`}
                          disabled={deactivate.isPending}
                          onClick={() => handleDeactivate(category)}
                        >
                          <BlockIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {categories.data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3}>Aucune catégorie.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Paper>
      )}

      {editing !== undefined && (
        <DocumentCategoryNameDialog
          initialName={editing?.name}
          submitting={mutation.isPending}
          error={mutation.isError ? documentCategoryErrorMessage(mutation.error) : null}
          onSubmit={handleSubmit}
          onEdit={mutation.reset}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
