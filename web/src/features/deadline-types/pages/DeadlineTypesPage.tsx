import {
  Alert,
  Button,
  Chip,
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
import PlaylistAddIcon from "@mui/icons-material/PlaylistAdd";
import { useState } from "react";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import useNotifSnack from "../../../hooks/useNotifSnack";
import {
  useCreateDeadlineType,
  useDeactivateDeadlineType,
  useDeadlineTypes,
  useUpdateDeadlineType,
} from "../api/deadlineTypeQueries";
import { deadlineTypeErrorMessage } from "../api/deadlineTypeErrors";
import AssignDeadlineTypeDialog from "../components/AssignDeadlineTypeDialog";
import DeadlineThresholdsSection from "../components/DeadlineThresholdsSection";
import DeadlineTypeDialog from "../components/DeadlineTypeDialog";
import type { DeadlineType, DeadlineTypeInput } from "../types/deadlineType";
import { formatPeriodicity, formatThresholds } from "../types/format";

/** `null` : création ; un type : modification ; `undefined` : dialogue fermé. */
type Editing = DeadlineType | null | undefined;

export default function DeadlineTypesPage() {
  const { notifySuccess, notifyError } = useNotifSnack();
  const [editing, setEditing] = useState<Editing>(undefined);

  const types = useDeadlineTypes(true);
  const create = useCreateDeadlineType();
  const update = useUpdateDeadlineType();
  const deactivate = useDeactivateDeadlineType();

  const [assigning, setAssigning] = useState<DeadlineType | undefined>(undefined);

  const mutation = editing ? update : create;

  const closeDialog = () => {
    setEditing(undefined);
    create.reset();
    update.reset();
  };

  const handleSubmit = (input: DeadlineTypeInput) => {
    const onSuccess = () => {
      notifySuccess(editing ? "Type d'échéance modifié" : "Type d'échéance ajouté");
      closeDialog();
    };
    if (editing) update.mutate({ id: editing.id, input }, { onSuccess });
    else {
      // À la création d'un type périodique, on propose de l'ajouter aux véhicules.
      create.mutate(input, {
        onSuccess: (created) => {
          onSuccess();
          if (!created.isOneOff) setAssigning(created);
        },
      });
    }
  };

  const handleDeactivate = (type: DeadlineType) =>
    deactivate.mutate(type.id, {
      onSuccess: () => notifySuccess(`Type « ${type.name} » désactivé`),
      onError: (error) => notifyError(deadlineTypeErrorMessage(error)),
    });

  return (
    <Stack gap={2} marginY={2}>
      <Stack direction="row" alignItems="center" gap={1}>
        <Typography variant="h5" flexGrow={1}>
          Types d'échéance
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing(null)}>
          Ajouter
        </Button>
      </Stack>
      <Alert severity="info">
        Un type désactivé n'est plus proposé mais reste affiché sur les échéances existantes. Sans périodicité, un type
        est ponctuel.
      </Alert>

      {types.isError ? (
        <ErrorHandler error={types.error} />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Nom</TableCell>
                <TableCell>Périodicité</TableCell>
                <TableCell>Seuils « proche »</TableCell>
                <TableCell>Statut</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {(types.data ?? []).map((type) => (
                <TableRow key={type.id}>
                  <TableCell>{type.name}</TableCell>
                  <TableCell>{formatPeriodicity(type)}</TableCell>
                  <TableCell>{formatThresholds(type)}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={type.isActive ? "Actif" : "Désactivé"}
                      color={type.isActive ? "success" : "default"}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Modifier">
                      <IconButton aria-label={`Modifier ${type.name}`} onClick={() => setEditing(type)}>
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    {type.isActive && !type.isOneOff && (
                      <Tooltip title="Ajouter à des véhicules">
                        <IconButton
                          aria-label={`Ajouter ${type.name} à des véhicules`}
                          onClick={() => setAssigning(type)}
                        >
                          <PlaylistAddIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                    {type.isActive && (
                      <Tooltip title="Désactiver">
                        <IconButton
                          aria-label={`Désactiver ${type.name}`}
                          disabled={deactivate.isPending}
                          onClick={() => handleDeactivate(type)}
                        >
                          <BlockIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {types.data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5}>Aucun type d'échéance.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Paper>
      )}

      <DeadlineThresholdsSection />

      {assigning && <AssignDeadlineTypeDialog type={assigning} onClose={() => setAssigning(undefined)} />}

      {editing !== undefined && (
        <DeadlineTypeDialog
          type={editing ?? undefined}
          submitting={mutation.isPending}
          error={mutation.isError ? deadlineTypeErrorMessage(mutation.error) : null}
          onSubmit={handleSubmit}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
