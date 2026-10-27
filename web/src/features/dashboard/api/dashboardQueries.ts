import { useQuery } from "@tanstack/react-query";
import type { DashboardFilter } from "../types/dashboard";
import { listDashboardDeadlines } from "./dashboardApi";

export const dashboardKeys = {
  all: ["dashboard"] as const,
  deadlines: (f: DashboardFilter) =>
    ["dashboard", "deadlines", f.vehicleId, f.vehicleTypeId, f.deadlineTypeId, f.status] as const,
};

export function useDashboardDeadlines(filter: DashboardFilter) {
  return useQuery({ queryKey: dashboardKeys.deadlines(filter), queryFn: () => listDashboardDeadlines(filter) });
}
