import { apiClient } from "../../../api/client";
import type { DashboardDeadline, DashboardFilter } from "../types/dashboard";

export async function listDashboardDeadlines(filter: DashboardFilter): Promise<DashboardDeadline[]> {
  const { data } = await apiClient.get<DashboardDeadline[]>("api/dashboard/deadlines", {
    params: {
      vehicleId: filter.vehicleId || undefined,
      vehicleTypeId: filter.vehicleTypeId || undefined,
      deadlineTypeId: filter.deadlineTypeId || undefined,
      status: filter.status || undefined,
    },
  });
  return data;
}
