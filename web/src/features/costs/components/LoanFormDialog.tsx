import { TextField } from "@mui/material";
import { useState } from "react";
import AmountField from "../../../shared/components/AmountField";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import { parseAmount } from "../../../shared/format/parseAmount";
import useFormErrors from "../../../shared/hooks/useFormErrors";
import { loanEndMonth, type LoanInput } from "../types/loan";
import { emptyLoanForm, isValidDuration, toLoanInput, type LoanFormValues } from "./loanForm";

interface LoanFormDialogProps {
  /** Absent pour un ajout. */
  initialValues?: LoanFormValues;
  submitting: boolean;
  error: string | null;
  onSubmit: (input: LoanInput) => void;
  onClose: () => void;
}

type Field = keyof LoanFormValues;

const amountCheck = (text: string, missing: string) => {
  const parsed = parseAmount(text);
  return parsed.error ?? (parsed.value === null && missing);
};

export default function LoanFormDialog({ initialValues, submitting, error, onSubmit, onClose }: LoanFormDialogProps) {
  const [values, setValues] = useState<LoanFormValues>(initialValues ?? emptyLoanForm);
  const { errors, validate, clear, fieldProps } = useFormErrors<Field>();
  const set = (patch: Partial<LoanFormValues>) => {
    setValues((v) => ({ ...v, ...patch }));
    clear(...(Object.keys(patch) as Field[]));
  };
  const durationInvalid = values.durationMonths !== "" && !isValidDuration(values.durationMonths);

  const durationHelp = durationInvalid
    ? "Nombre entier de mois, au moins 1."
    : values.startDate !== "" && isValidDuration(values.durationMonths)
      ? `Dernière mensualité : ${loanEndMonth(values.startDate, Number(values.durationMonths))}`
      : "Raccourcissable en cas de remboursement anticipé ou de vente";

  const handleSubmit = () => {
    const valid = validate({
      lender: values.lender.trim() === "" && "Indiquez l'organisme prêteur",
      borrowedAmount: amountCheck(values.borrowedAmount, "Indiquez le montant emprunté"),
      monthlyPayment: amountCheck(values.monthlyPayment, "Indiquez la mensualité"),
      startDate: values.startDate === "" && "Indiquez la date de début",
      durationMonths: !isValidDuration(values.durationMonths) && "Indiquez la durée en mois (nombre entier, au moins 1)",
    });
    if (valid) onSubmit(toLoanInput(values));
  };

  return (
    <FormDialog
      title={initialValues ? "Modifier l'emprunt" : "Ajouter un emprunt"}
      submitLabel={initialValues ? "Enregistrer" : "Ajouter"}
      submitting={submitting}
      error={error}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <TextField
        label="Organisme prêteur"
        required
        fullWidth
        margin="dense"
        value={values.lender}
        slotProps={{ htmlInput: { maxLength: 200 } }}
        onChange={(e) => set({ lender: e.target.value })}
        {...fieldProps("lender")}
      />
      <AmountField
        label="Montant emprunté (€)"
        required
        name="borrowedAmount"
        errorText={errors.borrowedAmount}
        value={values.borrowedAmount}
        onChange={(borrowedAmount) => set({ borrowedAmount })}
      />
      <AmountField
        label="Mensualité (€)"
        required
        name="monthlyPayment"
        errorText={errors.monthlyPayment}
        value={values.monthlyPayment}
        onChange={(monthlyPayment) => set({ monthlyPayment })}
      />
      <FrenchDateField
        layout="form"
        label="Date de début"
        required
        value={values.startDate}
        onChange={(startDate) => set({ startDate })}
        {...fieldProps("startDate")}
      />
      <TextField
        label="Durée (mois)"
        required
        fullWidth
        margin="dense"
        value={values.durationMonths}
        slotProps={{ htmlInput: { inputMode: "numeric", maxLength: 4 } }}
        onChange={(e) => set({ durationMonths: e.target.value })}
        {...fieldProps("durationMonths", durationHelp)}
        error={durationInvalid || errors.durationMonths !== undefined}
      />
    </FormDialog>
  );
}
