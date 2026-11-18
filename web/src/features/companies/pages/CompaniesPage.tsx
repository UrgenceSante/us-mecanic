import {
  Alert,
  CircularProgress,
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
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import BlockIcon from "@mui/icons-material/Block";
import EditIcon from "@mui/icons-material/Edit";
import { useState } from "react";
import ErrorHandler from "../../../components/Utils/Error/ErrorHandler";
import PageTitle from "../../../shared/components/PageTitle";
import useBelow from "../../../shared/hooks/useBelow";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { useCreateCompany, useCompanies, useDeactivateCompany, useRenameCompany } from "../api/companyQueries";
import { companyErrorMessage } from "../api/companyErrors";
import CompanyCards from "../components/CompanyCards";
import CompanyNameDialog from "../components/CompanyNameDialog";
import type { Company } from "../types/company";

/** `null` : création ; une société : renommage ; `undefined` : dialogue fermé. */
type Editing = Company | null | undefined;

export default function CompaniesPage() {
  const { notifySuccess, notifyError } = useNotifSnack();
  const compact = useBelow("sm");
  const [editing, setEditing] = useState<Editing>(undefined);

  const companies = useCompanies(true);
  const create = useCreateCompany();
  const rename = useRenameCompany();
  const deactivate = useDeactivateCompany();

  const mutation = editing ? rename : create;

  const closeDialog = () => {
    setEditing(undefined);
    create.reset();
    rename.reset();
  };

  const handleSubmit = (name: string) => {
    const onSuccess = () => {
      notifySuccess(editing ? "Société renommée" : "Société ajoutée");
      closeDialog();
    };
    if (editing) rename.mutate({ id: editing.id, name }, { onSuccess });
    else create.mutate(name, { onSuccess });
  };

  const handleDeactivate = (company: Company) =>
    deactivate.mutate(company.id, {
      onSuccess: () => notifySuccess(`Société « ${company.name} » désactivée`),
      onError: (error) => notifyError(companyErrorMessage(error)),
    });

  return (
    <Stack gap={2} marginY={2}>
      <Stack direction="row" alignItems="center" gap={1}>
        <PageTitle flexGrow={1}>Sociétés</PageTitle>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setEditing(null)}>
          Ajouter
        </Button>
      </Stack>
      <Alert severity="info">
        Une société désactivée n'est plus proposée comme propriétaire ni comme locataire, mais reste affichée sur les véhicules et locations qui la citent.
      </Alert>

      {companies.isError ? (
        <ErrorHandler error={companies.error} />
      ) : companies.isLoading ? (
        <CircularProgress aria-label="Chargement" sx={{ alignSelf: "center" }} />
      ) : compact ? (
        <Paper>
          <CompanyCards
            companies={companies.data ?? []}
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
              {(companies.data ?? []).map((company) => (
                <TableRow key={company.id}>
                  <TableCell>{company.name}</TableCell>
                  <TableCell>
                    <Chip
                      size="small"
                      label={company.isActive ? "Active" : "Désactivée"}
                      color={company.isActive ? "success" : "default"}
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Tooltip title="Renommer">
                      <IconButton aria-label={`Renommer ${company.name}`} onClick={() => setEditing(company)}>
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    {company.isActive && (
                      <Tooltip title="Désactiver">
                        <IconButton
                          aria-label={`Désactiver ${company.name}`}
                          disabled={deactivate.isPending}
                          onClick={() => handleDeactivate(company)}
                        >
                          <BlockIcon />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {companies.data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3}>Aucune société.</TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Paper>
      )}

      {editing !== undefined && (
        <CompanyNameDialog
          initialName={editing?.name}
          submitting={mutation.isPending}
          error={mutation.isError ? companyErrorMessage(mutation.error) : null}
          onSubmit={handleSubmit}
          onClose={closeDialog}
        />
      )}
    </Stack>
  );
}
