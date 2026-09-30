import { useParams } from "react-router";
import useGetMecanicLogById from "../../hooks/mecanicLog/useGetMecanicLogById";
import AsyncComponent from "../Utils/AsyncComponent";
import type { IMecanicLog } from "./IMecanicLog";
import LogResume from "./LogResume";
import { Box, Skeleton } from "@mui/material";

export default function AsyncMecanicLogResume() {
  const params = useParams();

  const request = useGetMecanicLogById(params.logId || "-1");

  const loadingSkeleton = (
    <Box>
      <Skeleton variant="rectangular" width="100%" height="100%" />
    </Box>
  );

  return (
    <AsyncComponent
      query={request}
      render={(data: IMecanicLog) => <LogResume log={data} />}
      renderLoading={loadingSkeleton}
    />
  );
}
