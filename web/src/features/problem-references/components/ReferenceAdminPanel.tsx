import {
  Button,
  Chip,
  FormControlLabel,
  IconButton,
  Paper,
  Stack,
  Switch,
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
import useNotifSnack from "../../../hooks/useNotifSnack";
import {
  useCreateProblemReference,
  useDeactivateProblemReference,
  useProblemReferences,
  useRenameProblemReference,
} from "../api/problemReferenceQueries";
import { problemReferenceErrorMessage } from "../api/problemReferenceErrors";
import type { ProblemReference, ProblemReferenceConfig } from "../types/problemReference";
import ReferenceNameDialog from "./ReferenceNameDialog";

/** `null` : création ; une valeur : renommage ; `undefined` : dialogue fermé. */
type Editing = ProblemReference | null | undefined;

export default function ReferenceAdminPanel({ config }: { config: ProblemReferenceConfig }) {
  const { kind } = config;
  const { notifySuccess, notifyError } = useNotifSnack();
  const [editing, setEditing] = useState<Editing>(undefined);
  const [showInactive, setShowInactive] = useState(false);

  const items = useProblemReferences(kind, showInactive);
  const create = useCreateProblemReference(kind);
  const rename = useRenameProblemReference(kind);
  const deactivate = useDeactivateProblemReference(kind);

  const mutation = editing ? rename : create;

  const closeDialog = () => {
    setEditing(undefined);
    create.reset();
    rename.reset();
  };

  const handleSubmit = (name: string) => {
    const onSuccess = () => {
      notifySuccess(editing ? "Valeur renommée" : "Valeur ajoutée");
      closeDialog();
    };
    if (editing) rename.mutate({ id: editing.id, name }, { onSuccess });
    else create.mutate(name, { onSuccess });
  };

  const handleDeactivate = (item: ProblemReference) =>
    deactivate.mutate(item.id, {
      onSuccess: () => notifySuccess(`« ${item.name} » désactivé`),
      onError: (error) => notifyError(problemReferenceErrorMessage(error, config)),
    });

  return (
    <Stack gap={2}>
      <Stack direction="row" alignItems="center" gap={1}>
        <FormControlLabel
          sx={{ flexGrow: 1 }}
          control={<Switch checked={showInactive} onChange={(e) => setShowInactive(e.target.checked)} />}
          label="Afficher les valeurs désactivées"
        />
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing(null)}>
          Ajouter
        </Button>
      </Stack>

      {items.isError ? (
        <ErrorHandler error={items.error} />
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
              {(items.data ?? []).map((item) => (
                <TableRow key={item.id}>
                  <TableCell>{item.name}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={item.isActive ? "Actif" : "Désactivé"}
                      color={item.isActive ? "success" : "default"}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Renommer">
                      <IconButton aria-label={`Renommer ${item.name}`} onClick={() => setEditing(item)}>
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    {item.isActive && (
                      <Tooltip title="Désactiver">
                        <IconButton
                          aria-label={`Désactiver ${item.name}`}
                          disabled={deactivate.isPending}
                          onClick={() => handleDeactivate(item)}
                        >
                          <BlockIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {items.data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3}>Aucune valeur.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Paper>
      )}

      {editing !== undefined && (
        <ReferenceNameDialog
          singular={config.singular}
          initialName={editing?.name}
          submitting={mutation.isPending}
          error={mutation.isError ? problemReferenceErrorMessage(mutation.error, config) : null}
          onSubmit={handleSubmit}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
