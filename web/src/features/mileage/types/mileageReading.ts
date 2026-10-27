export interface MileageReading {
  id: string;
  vehicleId: string;
  /** Date ISO AAAA-MM-JJ. */
  date: string;
  kilometers: number;
  reason: string | null;
  recordedAt: string;
}

export interface MileageReadingInput {
  date: string;
  kilometers: number;
  reason: string | null;
  confirmHighMileage: boolean;
}
