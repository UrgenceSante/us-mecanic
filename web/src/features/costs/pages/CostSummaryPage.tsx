import { Alert, CircularProgress, Stack, Typography } from "@mui/material";
import { useState } from "react";
import { useSearchParams } from "react-router";
import { useCostSummary } from "../api/costSummaryQueries";
import { costSummaryErrorMessage } from "../api/costSummaryErrors";
import CostCategoryBreakdown from "../components/CostCategoryBreakdown";
import CostPeriodTable from "../components/CostPeriodTable";
import CostSummaryCards from "../components/CostSummaryCards";
import CostSummaryFilters from "../components/CostSummaryFilters";
import { currentYearRange } from "../components/costSummaryFormat";
import type { CostSummaryParams } from "../types/costSummary";

export default function CostSummaryPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [dates, setDates] = useState(() => currentYearRange());
  const [granularity, setGranularity] = useState<CostSummaryParams["granularity"]>("month");
  const params: CostSummaryParams = { ...dates, granularity, vehicleId: searchParams.get("vehicleId") ?? "" };

  const summary = useCostSummary(params, params.from !== "" && params.to !== "");

  const handleChange = (next: CostSummaryParams) => {
    setDates({ from: next.from, to: next.to });
    setGranularity(next.granularity);
    if (next.vehicleId !== params.vehicleId) setSearchParams(next.vehicleId ? { vehicleId: next.vehicleId } : {});
  };

  const data = summary.data;
  const isEmpty = data !== undefined && data.total === 0 && data.periods.every((p) => p.total === 0);

  return (
    <Stack gap={2} marginY={2}>
      <Typography variant="h5">Coûts</Typography>
      <CostSummaryFilters value={params} onChange={handleChange} />
      {summary.isLoading && <CircularProgress aria-label="Chargement" sx={{ alignSelf: "center" }} />}
      {summary.isError && <Alert severity="error">{costSummaryErrorMessage(summary.error)}</Alert>}
      {data && (
        <>
          <CostSummaryCards summary={data} />
          {isEmpty ? (
            <Typography color="text.secondary">Aucune dépense sur cette période.</Typography>
          ) : (
            <>
              <CostPeriodTable periods={data.periods} granularity={granularity} />
              <CostCategoryBreakdown categories={data.categories} />
            </>
          )}
        </>
      )}
    </Stack>
  );
}
