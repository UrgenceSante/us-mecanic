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
import {
  useCreateVehicleType,
  useDeactivateVehicleType,
  useRenameVehicleType,
  useVehicleTypes,
} from "../api/vehicleTypeQueries";
import { vehicleTypeErrorMessage } from "../api/vehicleTypeErrors";
import VehicleTypeNameDialog from "../components/VehicleTypeNameDialog";
import type { VehicleType } from "../types/vehicleType";

/** `null` : création ; un type : renommage ; `undefined` : dialogue fermé. */
type Editing = VehicleType | null | undefined;

export default function VehicleTypesPage() {
  const { notifySuccess, notifyError } = useNotifSnack();
  const [editing, setEditing] = useState<Editing>(undefined);

  const types = useVehicleTypes(true);
  const create = useCreateVehicleType();
  const rename = useRenameVehicleType();
  const deactivate = useDeactivateVehicleType();

  const mutation = editing ? rename : create;

  const closeDialog = () => {
    setEditing(undefined);
    create.reset();
    rename.reset();
  };

  const handleSubmit = (name: string) => {
    const onSuccess = () => {
      notifySuccess(editing ? "Type renommé" : "Type ajouté");
      closeDialog();
    };
    if (editing) rename.mutate({ id: editing.id, name }, { onSuccess });
    else create.mutate(name, { onSuccess });
  };

  const handleDeactivate = (type: VehicleType) =>
    deactivate.mutate(type.id, {
      onSuccess: () => notifySuccess(`Type « ${type.name} » désactivé`),
      onError: (error) => notifyError(vehicleTypeErrorMessage(error)),
    });

  return (
    <Stack gap={2} marginY={2}>
      <Stack direction="row" alignItems="center" gap={1}>
        <Typography variant="h5" flexGrow={1}>
          Types de véhicule
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing(null)}>
          Ajouter
        </Button>
      </Stack>
      <Alert severity="info">
        Un type désactivé n'est plus proposé pour de nouveaux véhicules mais reste affiché sur ceux qui l'ont déjà.
      </Alert>

      {types.isError ? (
        <ErrorHandler error={types.error} />
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
              {(types.data ?? []).map((type) => (
                <TableRow key={type.id}>
                  <TableCell>{type.name}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={type.isActive ? "Actif" : "Désactivé"}
                      color={type.isActive ? "success" : "default"}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Renommer">
                      <IconButton aria-label={`Renommer ${type.name}`} onClick={() => setEditing(type)}>
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
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
                  <TableCell colSpan={3}>Aucun type de véhicule.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Paper>
      )}

      {editing !== undefined && (
        <VehicleTypeNameDialog
          initialName={editing?.name}
          submitting={mutation.isPending}
          error={mutation.isError ? vehicleTypeErrorMessage(mutation.error) : null}
          onSubmit={handleSubmit}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
