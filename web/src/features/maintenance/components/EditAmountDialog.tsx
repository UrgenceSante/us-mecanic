import { Alert, Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { useState, type FormEvent } from "react";
import AmountField from "../../../shared/components/AmountField";
import { parseAmount } from "../../../shared/format/parseAmount";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { Intervention } from "../types/intervention";

interface EditAmountDialogProps {
  intervention: Intervention;
  submitting: boolean;
  error: string | null;
  /** null = vider le montant. */
  onSubmit: (amount: number | null) => void;
  onClose: () => void;
}

/** Petit dialogue de modification du montant d'une intervention (champ vide = montant retiré). */
export default function EditAmountDialog({ intervention, submitting, error, onSubmit, onClose }: EditAmountDialogProps) {
  const [amount, setAmount] = useState(intervention.amount == null ? "" : String(intervention.amount).replace(".", ","));
  const parsed = parseAmount(amount);

  const canSubmit = !submitting && parsed.error === null;
  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (canSubmit) onSubmit(parsed.value);
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="xs" component="form" onSubmit={handleSubmit}>
      <DialogTitle>Modifier le montant</DialogTitle>
      <DialogContent>
        {error && (
          <Alert severity="error" sx={{ marginBottom: 2 }}>
            {error}
          </Alert>
        )}
        <DialogContentText>
          Intervention du {formatDateOnly(intervention.date)} ({intervention.work})
        </DialogContentText>
        <AmountField autoFocus value={amount} onChange={setAmount} hint="Laisser vide pour retirer le montant" />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Annuler</Button>
        <Button type="submit" variant="contained" disabled={!canSubmit}>
          Enregistrer
        </Button>
      </DialogActions>
    </Dialog>
  );
}
