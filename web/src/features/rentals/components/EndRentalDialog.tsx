import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Stack } from "@mui/material";
import { useState, type FormEvent } from "react";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import { todayLocalIso } from "../../mileage/types/date";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { rentalFieldError } from "../api/rentalErrors";
import type { Rental } from "../types/rental";

interface EndRentalDialogProps {
  rental: Rental;
  submitting: boolean;
  error: unknown;
  onConfirm: (endDate: string) => void;
  onClose: () => void;
}

/** Terminer une location : date de fin proposée au jour même, modifiable. */
export default function EndRentalDialog({ rental, submitting, error, onConfirm, onClose }: EndRentalDialogProps) {
  const [endDate, setEndDate] = useState(todayLocalIso());
  const failure = rentalFieldError(error);
  const onField = failure?.field === "endDate";

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (endDate) onConfirm(endDate);
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={handleSubmit} noValidate>
        <DialogTitle>Terminer la location</DialogTitle>
        <DialogContent>
          <Stack gap={2} paddingTop={1}>
            {failure && !onField && <Alert severity="error">{failure.message}</Alert>}
            <DialogContentText>
              Location à {rental.tenant.name} depuis le {formatDateOnly(rental.startDate)}.
            </DialogContentText>
            <FrenchDateField
              label="Date de fin"
              required
              value={endDate}
              onChange={setEndDate}
              error={onField}
              helperText={onField ? failure?.message : undefined}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={submitting || endDate === ""}>
            Terminer
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
