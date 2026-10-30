import AddIcon from "@mui/icons-material/Add";
import { Alert, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material";
import { useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import type { VehicleStatus } from "../../vehicles/types/vehicle";
import { loanErrorMessage } from "../api/loanErrors";
import { useAddLoan, useDeleteLoan, useLoans, useUpdateLoan } from "../api/loanQueries";
import type { Loan, LoanInput } from "../types/loan";
import DeleteLoanDialog from "./DeleteLoanDialog";
import LoanFormDialog from "./LoanFormDialog";
import { loanToFormValues } from "./loanForm";
import LoanList from "./LoanList";

interface VehicleLoansProps {
  vehicleId: string;
  status: VehicleStatus;
}

export default function VehicleLoans({ vehicleId, status }: VehicleLoansProps) {
  const { notifySuccess } = useNotifSnack();
  const loans = useLoans(vehicleId);
  const add = useAddLoan(vehicleId);
  const update = useUpdateLoan(vehicleId);
  const remove = useDeleteLoan(vehicleId);
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Loan | null>(null);
  const [deleting, setDeleting] = useState<Loan | null>(null);

  const closeForm = () => {
    setAdding(false);
    setEditing(null);
    add.reset();
    update.reset();
  };
  const closeDelete = () => {
    setDeleting(null);
    remove.reset();
  };
  const done = (message: string, close: () => void) => ({
    onSuccess: () => {
      notifySuccess(message);
      close();
    },
  });

  const submit = (input: LoanInput) =>
    editing
      ? update.mutate({ loanId: editing.id, input }, done("Emprunt modifié", closeForm))
      : add.mutate(input, done("Emprunt enregistré", closeForm));
  const confirmDelete = () => deleting && remove.mutate(deleting.id, done("Emprunt supprimé", closeDelete));
  const formMutation = editing ? update : add;

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack gap={1}>
        <Stack direction="row" alignItems="center" gap={2} flexWrap="wrap">
          <Typography variant="h6">Emprunts</Typography>
          <Stack flexGrow={1} />
          {status === "Retired" ? (
            <Typography variant="body2" color="text.secondary">
              Véhicule sorti du parc : ajout impossible.
            </Typography>
          ) : (
            <Button startIcon={<AddIcon />} onClick={() => setAdding(true)}>
              Ajouter un emprunt
            </Button>
          )}
        </Stack>
        {loans.isLoading && <CircularProgress size={24} aria-label="Chargement des emprunts" />}
        {loans.isError && <Alert severity="error">{loanErrorMessage(loans.error)}</Alert>}
        {loans.data?.length === 0 && <Typography color="text.secondary">Aucun emprunt enregistré pour ce véhicule.</Typography>}
        {loans.data && loans.data.length > 0 && <LoanList loans={loans.data} onEdit={setEditing} onDelete={setDeleting} />}
      </Stack>
      {(adding || editing) && (
        <LoanFormDialog
          initialValues={editing ? loanToFormValues(editing) : undefined}
          submitting={formMutation.isPending}
          error={formMutation.isError ? loanErrorMessage(formMutation.error) : null}
          onSubmit={submit}
          onClose={closeForm}
        />
      )}
      {deleting && (
        <DeleteLoanDialog
          loan={deleting}
          submitting={remove.isPending}
          error={remove.isError ? loanErrorMessage(remove.error) : null}
          onConfirm={confirmDelete}
          onClose={closeDelete}
        />
      )}
    </Paper>
  );
}
