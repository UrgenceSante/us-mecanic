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
