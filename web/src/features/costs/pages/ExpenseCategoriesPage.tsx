import {
  Alert,
  Button,
  Chip,
  CircularProgress,
  IconButton,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import BlockIcon from "@mui/icons-material/Block";
import EditIcon from "@mui/icons-material/Edit";
import { useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import {
  useCreateExpenseCategory,
  useDeactivateExpenseCategory,
  useExpenseCategories,
  useRenameExpenseCategory,
} from "../api/expenseCategoryQueries";
import { expenseCategoryErrorMessage } from "../api/expenseCategoryErrors";
import DeactivateCategoryDialog from "../components/DeactivateCategoryDialog";
import ExpenseCategoryNameDialog from "../components/ExpenseCategoryNameDialog";
import type { ExpenseCategory } from "../types/expenseCategory";

/** `null` : création ; une catégorie : renommage ; `undefined` : dialogue fermé. */
type Editing = ExpenseCategory | null | undefined;

export default function ExpenseCategoriesPage() {
  const { notifySuccess, notifyError } = useNotifSnack();
  const [editing, setEditing] = useState<Editing>(undefined);
  const [deactivating, setDeactivating] = useState<ExpenseCategory | undefined>(undefined);

  const categories = useExpenseCategories(true);
  const create = useCreateExpenseCategory();
  const rename = useRenameExpenseCategory();
  const deactivate = useDeactivateExpenseCategory();

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

  const handleDeactivate = () => {
    if (!deactivating) return;
    const category = deactivating;
    deactivate.mutate(category.id, {
      onSuccess: () => notifySuccess(`Catégorie « ${category.name} » désactivée`),
      onError: (error) => notifyError(expenseCategoryErrorMessage(error)),
      onSettled: () => setDeactivating(undefined),
    });
  };

  return (
    <Stack gap={2} marginY={2}>
      <Stack direction="row" alignItems="center" gap={1}>
        <Typography variant="h5" flexGrow={1}>
          Catégories de dépense
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing(null)}>
          Ajouter
        </Button>
      </Stack>
      <Alert severity="info">
        Une catégorie désactivée n'est plus proposée à la saisie mais reste affichée sur les dépenses existantes.
      </Alert>

      {categories.isPending && <CircularProgress aria-label="Chargement" sx={{ alignSelf: "center" }} />}
      {categories.isError && <Alert severity="error">{expenseCategoryErrorMessage(categories.error)}</Alert>}
      {categories.data && (
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
              {categories.data.map((category) => (
                <TableRow key={category.id}>
                  <TableCell>{category.name}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={category.isActive ? "Actif" : "Désactivé"}
                      color={category.isActive ? "success" : "default"}
                    />
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
                          onClick={() => setDeactivating(category)}
                        >
                          <BlockIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {categories.data.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3}>Aucune catégorie.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Paper>
      )}

      {editing !== undefined && (
        <ExpenseCategoryNameDialog
          initialName={editing?.name}
          submitting={mutation.isPending}
          error={mutation.isError ? expenseCategoryErrorMessage(mutation.error) : null}
          onSubmit={handleSubmit}
          onClose={closeDialog}
        />
      )}
      {deactivating && (
        <DeactivateCategoryDialog
          name={deactivating.name}
          submitting={deactivate.isPending}
          onConfirm={handleDeactivate}
          onClose={() => setDeactivating(undefined)}
        />
      )}
    </Stack>
  );
}
