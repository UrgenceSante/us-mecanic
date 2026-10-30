import { parseAmount } from "../../../shared/format/parseAmount";
import type { Loan, LoanInput } from "../types/loan";

export interface LoanFormValues {
  lender: string;
  borrowedAmount: string;
  monthlyPayment: string;
  startDate: string;
  durationMonths: string;
}

export const emptyLoanForm: LoanFormValues = { lender: "", borrowedAmount: "", monthlyPayment: "", startDate: "", durationMonths: "" };

const amountToText = (value: number) => String(value).replace(".", ",");

export function loanToFormValues(loan: Loan): LoanFormValues {
  return {
    lender: loan.lender,
    borrowedAmount: amountToText(loan.borrowedAmount),
    monthlyPayment: amountToText(loan.monthlyPayment),
    startDate: loan.startDate,
    durationMonths: String(loan.durationMonths),
  };
}

export function isValidDuration(text: string): boolean {
  return /^\d+$/.test(text.trim()) && Number(text) >= 1;
}

/** Formulaire complet et valide ? Tous les champs sont obligatoires. */
export function isLoanFormValid(values: LoanFormValues): boolean {
  return (
    values.lender.trim() !== "" &&
    parseAmount(values.borrowedAmount).value !== null &&
    parseAmount(values.monthlyPayment).value !== null &&
    values.startDate !== "" &&
    isValidDuration(values.durationMonths)
  );
}

export function toLoanInput(values: LoanFormValues): LoanInput {
  return {
    lender: values.lender.trim(),
    borrowedAmount: parseAmount(values.borrowedAmount).value,
    monthlyPayment: parseAmount(values.monthlyPayment).value,
    startDate: values.startDate,
    durationMonths: Number(values.durationMonths),
  };
}
