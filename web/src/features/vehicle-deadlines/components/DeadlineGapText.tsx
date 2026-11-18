import { Typography } from "@mui/material";
import { deadlineGap, todayInParis, type DeadlineGapInput } from "../types/deadlineGap";

/** Écart de l'échéance (« dépassée depuis 12 j », « dans 6 j »…), en rouge foncé quand elle est dépassée. */
export default function DeadlineGapText(props: DeadlineGapInput) {
  const gap = deadlineGap(props, todayInParis());
  if (!gap) return null;
  const overdue = gap.overdueBy.length > 0;
  return (
    <Typography component="span" variant="body2" fontWeight={overdue ? "bold" : undefined} color={overdue ? "error" : "text.secondary"}>
      {gap.text}
    </Typography>
  );
}
