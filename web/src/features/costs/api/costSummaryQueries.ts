import { useQuery } from "@tanstack/react-query";
import type { CostSummaryParams } from "../types/costSummary";
import { getCostSummary } from "./costSummaryApi";

export const costSummaryKeys = {
  summary: (p: CostSummaryParams) =>
    ["costs", "summary", { from: p.from, to: p.to, granularity: p.granularity, vehicleId: p.vehicleId }] as const,
};

export function useCostSummary(params: CostSummaryParams, enabled = true) {
  return useQuery({ queryKey: costSummaryKeys.summary(params), queryFn: () => getCostSummary(params), enabled });
}
