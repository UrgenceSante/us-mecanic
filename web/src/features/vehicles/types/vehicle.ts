export type VehicleStatus = "Active" | "Immobilized" | "Retired";

export interface Vehicle {
  id: string;
  licensePlate: string;
  brand: string | null;
  model: string | null;
  /** Date ISO AAAA-MM-JJ. */
  firstRegistrationDate: string | null;
  vin: string | null;
  internalNumber: string | null;
  status: VehicleStatus;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleFilter {
  search?: string;
  status?: VehicleStatus;
}

export interface VehicleDetailsInput {
  brand: string | null;
  model: string | null;
  firstRegistrationDate: string | null;
  vin: string | null;
  internalNumber: string | null;
}

export interface RegisterVehicleCommand {
  licensePlate: string;
  details: VehicleDetailsInput;
}

export type ImportOutcome = "Created" | "Updated" | "Unchanged" | "Error";

export interface VehicleImportLine {
  line: number;
  licensePlate: string | null;
  outcome: ImportOutcome;
  errorCode: string | null;
  message: string | null;
}

export interface VehicleImportReport {
  dryRun: boolean;
  created: number;
  updated: number;
  unchanged: number;
  errors: number;
  lines: VehicleImportLine[];
}

export const vehicleStatusLabels: Record<VehicleStatus, string> = {
  Active: "En service",
  Immobilized: "Immobilisé",
  Retired: "Sorti du parc",
};
