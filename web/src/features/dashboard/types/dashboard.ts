import type { DeadlineStatus } from "../../vehicle-deadlines/types/vehicleDeadline";

export interface DashboardDeadline {
  id: string;
  vehicle: {
    id: string;
    licensePlate: string;
    internalNumber: string | null;
    typeId: string | null;
    typeName: string | null;
  };
  deadlineType: { id: string; name: string };
  status: DeadlineStatus;
  /** Date ISO AAAA-MM-JJ. */
  dueDate: string | null;
  dueKm: number | null;
  /** Km actuel du véhicule, null sans relevé. */
  currentKm: number | null;
}

/** Filtres du tableau de bord ; une chaîne vide signifie « tous ». */
export interface DashboardFilter {
  vehicleId: string;
  vehicleTypeId: string;
  deadlineTypeId: string;
  status: "" | "Overdue" | "DueSoon";
}

export const emptyDashboardFilter: DashboardFilter = { vehicleId: "", vehicleTypeId: "", deadlineTypeId: "", status: "" };
