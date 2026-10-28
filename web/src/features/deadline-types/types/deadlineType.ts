export interface DeadlineType {
  id: string;
  name: string;
  defaultIntervalMonths: number | null;
  defaultIntervalKm: number | null;
  dueSoonDays: number | null;
  dueSoonKm: number | null;
  isOneOff: boolean;
  isActive: boolean;
}

/** Corps de création / modification : le PUT remplace tout, tous les champs sont toujours envoyés. */
export interface DeadlineTypeInput {
  name: string;
  defaultIntervalMonths: number | null;
  defaultIntervalKm: number | null;
  dueSoonDays: number | null;
  dueSoonKm: number | null;
}

export interface DeadlineThresholds {
  dueSoonDays: number;
  dueSoonKm: number;
}

/** Corps de l'ajout en lot : types de véhicule visés, dernière réalisation facultative (sinon aujourd'hui côté API). */
export interface AssignDeadlineTypeInput {
  vehicleTypeIds: string[];
  lastDoneDate: string | null;
}

export interface AssignDeadlineTypeError {
  vehicleId: string;
  licensePlate: string;
  code: string;
  message: string;
}

/** Bilan de l'ajout en lot. */
export interface AssignDeadlineTypeResult {
  added: number;
  skippedAlreadyActive: number;
  skippedRetired: number;
  errors: AssignDeadlineTypeError[];
}
