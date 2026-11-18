import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import FindInPageIcon from "@mui/icons-material/FindInPage";
import ReportProblemIcon from "@mui/icons-material/ReportProblem";
import type { ReactElement } from "react";
import StatusChip, { type StatusTone } from "../../../shared/components/StatusChip";
import { problemStatusLabels } from "../types/labels";
import type { ProblemStatus } from "../types/problem";

export const problemStatusChips: Record<ProblemStatus, { tone: StatusTone; icon: ReactElement }> = {
  Reported: { tone: "warning", icon: <ReportProblemIcon /> },
  Analyzed: { tone: "info", icon: <FindInPageIcon /> },
  Resolved: { tone: "success", icon: <CheckCircleIcon /> },
};

export default function ProblemStatusChip({ status }: { status: ProblemStatus }) {
  return <StatusChip {...problemStatusChips[status]} label={problemStatusLabels[status]} />;
}
