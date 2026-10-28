export interface InterventionDeadline {
  id: string;
  typeName: string | null;
}

export interface Intervention {
  id: string;
  vehicleId: string;
  /** Date ISO AAAA-MM-JJ. */
  date: string;
  kilometers: number | null;
  garage: { id: string; name: string } | null;
  work: string;
  recordedAt: string;
  deadlines: InterventionDeadline[];
}

export interface InterventionInput {
  date: string;
  kilometers: number | null;
  garageId: string | null;
  work: string;
  deadlineIds: string[];
  reason: string | null;
  confirmHighMileage: boolean;
}
