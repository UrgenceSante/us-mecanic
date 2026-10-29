export type ProblemStatus = "Reported" | "Analyzed" | "Resolved";
export type BlockingLevel = "Immobilizing" | "Unanalyzed" | "OverdueAction" | "Other";

export interface ProblemAnalysis {
  natureId: string;
  elementId: string;
  text: string;
  immobilized: boolean;
  analyzedAt: string;
}

export interface ProblemAction {
  id: string;
  problemId: string;
  actorId: string;
  typeId: string;
  constraintId: string;
  dueDate: string | null;
  comment: string | null;
  interventionId: string | null;
  isClosed: boolean;
  closedAt: string | null;
  recordedAt: string;
}

export interface Problem {
  id: string;
  vehicleId: string;
  date: string;
  description: string;
  author: string;
  status: ProblemStatus;
  recordedAt: string;
  analysis: ProblemAnalysis | null;
  actions: ProblemAction[];
}

export interface FleetProblem {
  problem: Problem;
  vehicle: { id: string; licensePlate: string };
  nature: { id: string; name: string } | null;
  blockingLevel: BlockingLevel;
}

/** Filtres de la liste du parc ; chaîne vide = pas de filtre. */
export interface FleetProblemFilter {
  status: ProblemStatus | "";
  vehicleId: string;
  natureId: string;
}

export const emptyFleetProblemFilter: FleetProblemFilter = {
  status: "",
  vehicleId: "",
  natureId: "",
};

export interface AnalyzeProblemInput {
  natureId: string;
  elementId: string;
  analysis: string;
  immobilized: boolean;
}

export interface AddProblemActionInput {
  actorId: string;
  typeId: string;
  constraintId: string;
  dueDate: string | null;
  comment: string | null;
  interventionId: string | null;
}

/** La modification remplace toujours les trois champs. */
export interface UpdateProblemActionInput {
  dueDate: string | null;
  comment: string | null;
  interventionId: string | null;
}
