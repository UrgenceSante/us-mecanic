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
import { useState } from "react";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { useCreateGarage, useDeactivateGarage, useGarages, useRenameGarage } from "../api/garageQueries";
import { garageErrorMessage } from "../api/garageErrors";
import GarageNameDialog from "../components/GarageNameDialog";
import type { Garage } from "../types/garage";

/** `null` : création ; un garage : renommage ; `undefined` : dialogue fermé. */
type Editing = Garage | null | undefined;

export default function GaragesPage() {
  const { notifySuccess, notifyError } = useNotifSnack();
  const [editing, setEditing] = useState<Editing>(undefined);

  const garages = useGarages(true);
  const create = useCreateGarage();
  const rename = useRenameGarage();
  const deactivate = useDeactivateGarage();

  const mutation = editing ? rename : create;

  const closeDialog = () => {
    setEditing(undefined);
    create.reset();
    rename.reset();
  };

  const handleSubmit = (name: string) => {
    const onSuccess = () => {
      notifySuccess(editing ? "Garage renommé" : "Garage ajouté");
      closeDialog();
    };
    if (editing) rename.mutate({ id: editing.id, name }, { onSuccess });
    else create.mutate(name, { onSuccess });
  };

  const handleDeactivate = (garage: Garage) =>
    deactivate.mutate(garage.id, {
      onSuccess: () => notifySuccess(`Garage « ${garage.name} » désactivé`),
      onError: (error) => notifyError(garageErrorMessage(error)),
    });

  return (
    <Stack gap={2} marginY={2}>
      <Stack direction="row" alignItems="center" gap={1}>
        <Typography variant="h5" flexGrow={1}>
          Garages
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing(null)}>
          Ajouter
        </Button>
      </Stack>
      <Alert severity="info">
        Un garage désactivé n'est plus proposé à la saisie mais reste affiché sur les interventions existantes.
      </Alert>

      {garages.isError ? (
        <ErrorHandler error={garages.error} />
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
              {(garages.data ?? []).map((garage) => (
                <TableRow key={garage.id}>
                  <TableCell>{garage.name}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={garage.isActive ? "Actif" : "Désactivé"}
                      color={garage.isActive ? "success" : "default"}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Renommer">
                      <IconButton aria-label={`Renommer ${garage.name}`} onClick={() => setEditing(garage)}>
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    {garage.isActive && (
                      <Tooltip title="Désactiver">
                        <IconButton
                          aria-label={`Désactiver ${garage.name}`}
                          disabled={deactivate.isPending}
                          onClick={() => handleDeactivate(garage)}
                        >
                          <BlockIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {garages.data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3}>Aucun garage.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Paper>
      )}

      {editing !== undefined && (
        <GarageNameDialog
          initialName={editing?.name}
          submitting={mutation.isPending}
          error={mutation.isError ? garageErrorMessage(mutation.error) : null}
          onSubmit={handleSubmit}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
