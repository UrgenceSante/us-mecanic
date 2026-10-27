import type { ChipProps } from "@mui/material";

export type DeadlineStatus = "UpToDate" | "DueSoon" | "Overdue";

export interface VehicleDeadline {
  id: string;
  vehicleId: string;
  deadlineTypeId: string;
  typeName: string | null;
  intervalMonths: number | null;
  intervalKm: number | null;
  /** Date ISO AAAA-MM-JJ. */
  lastDoneDate: string | null;
  lastDoneKm: number | null;
  dueDate: string | null;
  dueKm: number | null;
  isClosed: boolean;
  /** Null pour une échéance close. */
  status: DeadlineStatus | null;
}

export interface VehicleDeadlineInput {
  deadlineTypeId: string;
  intervalMonths: number | null;
  intervalKm: number | null;
  lastDoneDate: string | null;
  lastDoneKm: number | null;
  dueDate: string | null;
  dueKm: number | null;
}

export interface DeadlineCompletionInput {
  date: string;
  kilometers: number | null;
  comment: string | null;
  reason: string | null;
  confirmHighMileage: boolean;
}

export const deadlineStatusLabels: Record<DeadlineStatus, string> = {
  UpToDate: "À jour",
  DueSoon: "Proche",
  Overdue: "Dépassée",
};

export const deadlineStatusColors: Record<DeadlineStatus, ChipProps["color"]> = {
  UpToDate: "success",
  DueSoon: "warning",
  Overdue: "error",
};
