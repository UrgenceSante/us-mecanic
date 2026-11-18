import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack } from "@mui/material";
import { useState, type FormEvent } from "react";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import CompanySelect from "../../companies/components/CompanySelect";
import { rentalFieldError, type RentalErrorField } from "../api/rentalErrors";
import type { Rental, RentalInput } from "../types/rental";

interface RentalFormDialogProps {
  /** Location à modifier ; absente pour un ajout. */
  rental?: Rental;
  submitting: boolean;
  error: unknown;
  onSubmit: (input: RentalInput) => void;
  onClose: () => void;
}

export default function RentalFormDialog({ rental, submitting, error, onSubmit, onClose }: RentalFormDialogProps) {
  const [tenantId, setTenantId] = useState(rental?.tenant.id ?? "");
  const [startDate, setStartDate] = useState(rental?.startDate ?? "");
  const [endDate, setEndDate] = useState(rental?.endDate ?? "");
  const valid = tenantId !== "" && startDate !== "";
  const failure = rentalFieldError(error);
  const fieldProps = (field: RentalErrorField, hint?: string) => ({
    error: failure?.field === field,
    helperText: failure?.field === field ? failure.message : hint,
  });

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (valid) onSubmit({ tenantCompanyId: tenantId, startDate, endDate: endDate || null });
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit} noValidate>
        <DialogTitle>{rental ? "Modifier la location" : "Ajouter une location"}</DialogTitle>
        <DialogContent>
          <Stack gap={2} paddingTop={1}>
            {failure?.field === "general" && <Alert severity="error">{failure.message}</Alert>}
            <CompanySelect
              label="Société locataire"
              required
              value={tenantId}
              onChange={setTenantId}
              emptyLabel="Choisir une société"
              allowEmpty={false}
              current={rental?.tenant}
              {...fieldProps("tenant")}
              autoFocus
            />
            <FrenchDateField
              label="Date de début"
              required
              value={startDate}
              onChange={setStartDate}
              {...fieldProps("startDate")}
            />
            <FrenchDateField
              label="Date de fin (facultative)"
              value={endDate}
              onChange={setEndDate}
              {...fieldProps("endDate", "Laissez vide si la fin n'est pas connue")}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={submitting || !valid}>
            {rental ? "Enregistrer" : "Ajouter"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
