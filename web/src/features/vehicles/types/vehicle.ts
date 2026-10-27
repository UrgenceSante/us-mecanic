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
  typeId: string | null;
  typeName: string | null;
  status: VehicleStatus;
  /** Km actuel (relevé le plus récent par date), null sans relevé. */
  currentMileage: number | null;
  /** Date ISO AAAA-MM-JJ du km actuel. */
  currentMileageDate: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleFilter {
  search?: string;
  status?: VehicleStatus;
  /** Id du type de véhicule. */
  type?: string;
}

export interface VehicleDetailsInput {
  brand: string | null;
  model: string | null;
  firstRegistrationDate: string | null;
  vin: string | null;
  internalNumber: string | null;
  /** Toujours envoyé : le PUT remplace tout, un typeId absent efface le type. */
  typeId: string | null;
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
