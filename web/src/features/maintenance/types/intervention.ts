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
  /** Montant en euros, null si non renseigné. */
  amount: number | null;
  recordedAt: string;
  deadlines: InterventionDeadline[];
}

export interface InterventionInput {
  date: string;
  kilometers: number | null;
  garageId: string | null;
  work: string;
  amount: number | null;
  deadlineIds: string[];
  reason: string | null;
  confirmHighMileage: boolean;
}
