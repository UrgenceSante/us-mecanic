import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";
import AmountField from "../../../shared/components/AmountField";
import { loanEndMonth, type LoanInput } from "../types/loan";
import { emptyLoanForm, isLoanFormValid, isValidDuration, toLoanInput, type LoanFormValues } from "./loanForm";

interface LoanFormDialogProps {
  /** Absent pour un ajout. */
  initialValues?: LoanFormValues;
  submitting: boolean;
  error: string | null;
  onSubmit: (input: LoanInput) => void;
  onClose: () => void;
}

export default function LoanFormDialog({ initialValues, submitting, error, onSubmit, onClose }: LoanFormDialogProps) {
  const [values, setValues] = useState<LoanFormValues>(initialValues ?? emptyLoanForm);
  const set = (patch: Partial<LoanFormValues>) => setValues((v) => ({ ...v, ...patch }));
  const durationInvalid = values.durationMonths !== "" && !isValidDuration(values.durationMonths);

  const durationHelp = durationInvalid
    ? "Nombre entier de mois, au moins 1."
    : values.startDate !== "" && isValidDuration(values.durationMonths)
      ? `Dernière mensualité : ${loanEndMonth(values.startDate, Number(values.durationMonths))}`
      : "Raccourcissable en cas de remboursement anticipé ou de vente";

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (isLoanFormValid(values)) onSubmit(toLoanInput(values));
  };

  return (
    <Dialog open onClose={onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{initialValues ? "Modifier l'emprunt" : "Ajouter un emprunt"}</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 2 }}>
              {error}
            </Alert>
          )}
          <TextField
            label="Organisme prêteur"
            required
            fullWidth
            margin="dense"
            value={values.lender}
            slotProps={{ htmlInput: { maxLength: 200 } }}
            onChange={(e) => set({ lender: e.target.value })}
          />
          <AmountField label="Montant emprunté (€)" required value={values.borrowedAmount} onChange={(v) => set({ borrowedAmount: v })} />
          <AmountField label="Mensualité (€)" required value={values.monthlyPayment} onChange={(v) => set({ monthlyPayment: v })} />
          <TextField
            label="Date de début"
            type="date"
            required
            fullWidth
            margin="dense"
            value={values.startDate}
            slotProps={{ inputLabel: { shrink: true } }}
            onChange={(e) => set({ startDate: e.target.value })}
          />
          <TextField
            label="Durée (mois)"
            required
            fullWidth
            margin="dense"
            value={values.durationMonths}
            error={durationInvalid}
            helperText={durationHelp}
            slotProps={{ htmlInput: { inputMode: "numeric", maxLength: 4 } }}
            onChange={(e) => set({ durationMonths: e.target.value })}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={submitting || !isLoanFormValid(values)}>
            {initialValues ? "Enregistrer" : "Ajouter"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
