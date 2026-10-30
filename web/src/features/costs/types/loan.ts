export interface Loan {
  id: string;
  vehicleId: string;
  lender: string;
  borrowedAmount: number;
  monthlyPayment: number;
  /** Date au format AAAA-MM-JJ. */
  startDate: string;
  durationMonths: number;
  recordedAt: string;
}

export interface LoanInput {
  lender: string;
  borrowedAmount: number | null;
  monthlyPayment: number | null;
  startDate: string;
  durationMonths: number;
}

/** Mois de la dernière mensualité (MM/AAAA) : une mensualité par mois à partir du mois de début. */
export function loanEndMonth(startDate: string, durationMonths: number): string {
  const [year, month] = startDate.split("-").map(Number);
  const index = year * 12 + (month - 1) + durationMonths - 1;
  const endYear = Math.floor(index / 12);
  const endMonth = (index % 12) + 1;
  return `${String(endMonth).padStart(2, "0")}/${endYear}`;
}
