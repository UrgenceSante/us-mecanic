import { apiClient } from "../../../api/client";
import type { CostSummary, CostSummaryParams } from "../types/costSummary";

export async function getCostSummary(params: CostSummaryParams): Promise<CostSummary> {
  const { data } = await apiClient.get<CostSummary>("api/costs/summary", {
    params: { from: params.from, to: params.to, granularity: params.granularity, vehicleId: params.vehicleId || undefined },
  });
  return data;
}
