export type CostFamily = "OneOff" | "Running" | "Loan";
export type CostGranularity = "month" | "year";

export interface CostFamilyTotal {
  family: CostFamily;
  total: number;
}

export interface CostCategoryTotal {
  family: CostFamily;
  categoryId: string | null;
  category: string;
  total: number;
}

export interface CostPeriod {
  /** « yyyy-MM » ou « yyyy ». */
  period: string;
  start: string;
  total: number;
  families: CostFamilyTotal[];
  categories: CostCategoryTotal[];
}

export interface CostPerKm {
  kilometers: number;
  cost: number;
  /** €/km, null si non calculable. */
  value: number | null;
  computable: boolean;
  excludedVehicles: number;
}

export interface CostSummary {
  from: string;
  to: string;
  vehicleId: string | null;
  total: number;
  families: CostFamilyTotal[];
  categories: CostCategoryTotal[];
  periods: CostPeriod[];
  costPerKm: CostPerKm;
}

export interface CostSummaryParams {
  /** Dates ISO AAAA-MM-JJ. */
  from: string;
  to: string;
  granularity: CostGranularity;
  vehicleId: string;
}
